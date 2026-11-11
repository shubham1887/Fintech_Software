using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;

namespace FintechApp.Authorization
{
    // Authenticates the JWT and (optionally) authorizes by user-type.
    //   [JwtAuthorization]                                  → any authenticated user
    //   [JwtAuthorization(UserTypeCode.Admin)]              → Admin only
    //   [JwtAuthorization(UserTypeCode.Admin, UserTypeCode.ApiUser)] → Admin or API user
    // Also enforces that the request-body KeyData identity matches the signed token (anti-impersonation).
    public class JwtAuthorizationAttribute : ActionFilterAttribute
    {
        private readonly int[] _allowedUserTypeIds;

        public JwtAuthorizationAttribute(params int[] allowedUserTypeIds)
        {
            _allowedUserTypeIds = allowedUserTypeIds ?? Array.Empty<int>();
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                // Skip authorization for endpoints marked with AllowAnonymous
                var skipAuthorization = context.ActionDescriptor.EndpointMetadata
                    .Any(em => em.GetType() == typeof(AllowAnonymousAttribute));

                if (skipAuthorization)
                {
                    await next();
                    return;
                }

                // Get Authorization header
                if (!context.HttpContext.Request.Headers.TryGetValue("Authorization", out var authHeader))
                {
                    context.Result = Unauthorized(Messages.Common.Message.Unauthorized);
                    return;
                }

                var token = authHeader.ToString().Replace("Bearer ", string.Empty);
                if (string.IsNullOrEmpty(token))
                {
                    context.Result = Unauthorized(Messages.Common.Message.InvalidSession);
                    return;
                }

                // Validate JWT token
                var jwtService = context.HttpContext.RequestServices.GetService<JwtTokenService>();
                if (jwtService == null)
                {
                    context.Result = new JsonResult(ResponseProcess.GetErrorResponse()) { StatusCode = 500 };
                    return;
                }

                var principal = jwtService.ValidateToken(token);
                if (principal == null)
                {
                    context.Result = Unauthorized(Messages.Common.Message.SessionExpired);
                    return;
                }

                // Extract user information from token (signed, trustworthy)
                var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
                var userTypeIdClaim = principal.Claims.FirstOrDefault(c => c.Type == "userTypeId")?.Value;
                var userTypeClaim = principal.Claims.FirstOrDefault(c => c.Type == "userType")?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int tokenUserId))
                {
                    context.Result = Unauthorized(Messages.Common.Message.InvalidSession);
                    return;
                }
                int.TryParse(userTypeIdClaim, out int tokenUserTypeId);

                // Session generation. The token is signed and unexpired, but that only says it was
                // issued - not that the session it belongs to is still open. Signing out (and
                // changing a password) moves UserMaster.SessionEpoch on by one, so a token minted
                // before that is refused here even though it would validate perfectly.
                //
                // Without this, "log out" revoked only the refresh token and the access token kept
                // working for the rest of its hour: a second tab stayed able to transact after the
                // first one signed out, and so did any copy of the token taken off the wire.
                //
                // A token with no generation claim at all predates this check, and is treated as
                // generation 0 - so sessions already open when this shipped keep working until
                // their holder signs out once, rather than every user being kicked out on deploy.
                var users = context.HttpContext.RequestServices.GetService<IUserRepository>();
                if (users != null)
                {
                    var tokenUser = await users.GetUserById(tokenUserId);
                    if (tokenUser == null || !tokenUser.IsActive)
                    {
                        // Deleted or deactivated mid-session. The row is gone or switched off, so
                        // the token is the only thing still saying this account may act.
                        context.Result = Unauthorized(Messages.Common.Message.SessionExpired);
                        return;
                    }

                    int.TryParse(
                        principal.Claims.FirstOrDefault(c => c.Type == JwtTokenService.SessionEpochClaim)?.Value,
                        out int tokenEpoch);
                    if (tokenEpoch != tokenUser.SessionEpoch)
                    {
                        context.Result = Unauthorized(Messages.Common.Message.SessionExpired);
                        return;
                    }
                }

                // Role authorization (only when the attribute specifies allowed user-types)
                if (_allowedUserTypeIds.Length > 0 && !_allowedUserTypeIds.Contains(tokenUserTypeId))
                {
                    context.Result = Forbidden(Messages.Common.Message.NoPermission);
                    return;
                }

                // Central deny-by-default policy: the caller's role must be granted this
                // controller/action (user-type-wise access; everything else is denied). The
                // attribute's own args (above) can only further restrict, never widen this.
                var policy = context.HttpContext.RequestServices.GetService<IApiAccessPolicyService>();
                if (policy != null && context.ActionDescriptor is ControllerActionDescriptor cad)
                {
                    if (!policy.IsAllowed(cad.ControllerName, EndpointName(cad), tokenUserTypeId))
                    {
                        context.Result = Forbidden(Messages.Common.Message.NoPermission);
                        return;
                    }
                }

                // Anti-impersonation: the body KeyData identity must match the signed token identity.
                // (Controllers use keyDataModel.UserID/UserTypeID for business logic — without this check a
                // caller with a valid token could act as another user by changing the body.)
                if (!CrossCheckKeyData(context, tokenUserId, tokenUserTypeId, out var crossCheckError))
                {
                    context.Result = Unauthorized(crossCheckError);
                    return;
                }

                // Data-scope (hierarchy) gate. ApiAccessPolicy above decides WHICH endpoint a role
                // may call; this decides WHOSE rows it may name once inside. The request body is
                // walked generically (HierarchyScopePolicy), so every controller that uses the
                // standard field names — UserID / TargetUserID / ParentID / ReceiverID … — is
                // covered without touching it: a retailer can no longer read or act on a user in
                // another distributor's branch by putting that id in the payload.
                var scopeService = context.HttpContext.RequestServices.GetService<IUserScopeService>();
                if (scopeService != null && context.ActionDescriptor is ControllerActionDescriptor scopeCad)
                {
                    var scopeResult = await scopeService.CheckRequestJson(
                        scopeCad.ControllerName, EndpointName(scopeCad), tokenUserId, tokenUserTypeId,
                        DecryptRequestBody(context));
                    if (!scopeResult.Allowed)
                    {
                        context.Result = Forbidden(Messages.Common.Message.NoPermission);
                        return;
                    }
                }

                // Set user information in HttpContext for controllers to access
                context.HttpContext.Items["UserId"] = tokenUserId;
                context.HttpContext.Items["UserTypeId"] = tokenUserTypeId;
                context.HttpContext.Items["UserType"] = userTypeClaim;
                context.HttpContext.Items["Principal"] = principal;

                await next();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                context.Result = new JsonResult(ResponseProcess.GetErrorResponse()) { StatusCode = 500 };
            }
        }

        // The endpoint name the CLIENT calls — the last segment of the route template ("Select" in
        // [HttpPost("Select")]) — falling back to the C# method name. Both access policies are keyed
        // by endpoint, and a handful of actions have a method name that differs from their route
        // (Users/Select is method SelectAll); keying on the method name silently resolved those to
        // the deny-by-default Admin set, locking out roles the policy meant to allow.
        private static string EndpointName(ControllerActionDescriptor descriptor)
        {
            var template = descriptor.AttributeRouteInfo?.Template;
            if (string.IsNullOrWhiteSpace(template)) return descriptor.ActionName;

            var last = template.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            // Fall back when the action carries no sub-route of its own (the template then ends at
            // the controller), or the last segment is a route parameter / token ("{id}", "[action]").
            if (string.IsNullOrWhiteSpace(last) || last.Contains('{') || last.Contains('[')
                || last.Equals(descriptor.ControllerName, StringComparison.OrdinalIgnoreCase))
                return descriptor.ActionName;
            return last;
        }

        // Decrypts the bound APIRequest.Request payload so the hierarchy-scope gate can inspect it.
        // Returns empty when there is no body, or it cannot be decrypted (the controller will then
        // reject it on its own).
        private static string DecryptRequestBody(ActionExecutingContext context)
        {
            var apiRequest = context.ActionArguments.Values.OfType<APIRequest>().FirstOrDefault();
            if (apiRequest == null || string.IsNullOrWhiteSpace(apiRequest.Request)) return string.Empty;
            try
            {
                return Crypto(context).Decrypt(apiRequest.Request);
            }
            catch
            {
                return string.Empty;
            }
        }

        private static EncryptDecrypt Crypto(ActionExecutingContext context)
        {
            var config = context.HttpContext.RequestServices.GetService<IConfiguration>();
            return new EncryptDecrypt(config?["Encryption:Key"] ?? string.Empty, config?["Encryption:IV"] ?? string.Empty);
        }

        // Verifies the already-bound APIRequest's encrypted KeyData resolves to the same user as the token.
        // Returns true when there is no KeyData to check (e.g. query-only endpoints) or it matches.
        private static bool CrossCheckKeyData(ActionExecutingContext context, int tokenUserId, int tokenUserTypeId, out string error)
        {
            error = string.Empty;
            var apiRequest = context.ActionArguments.Values.OfType<APIRequest>().FirstOrDefault();
            if (apiRequest == null || string.IsNullOrWhiteSpace(apiRequest.KeyData))
                return true; // nothing to cross-check

            try
            {
                var config = context.HttpContext.RequestServices.GetService<IConfiguration>();
                var ed = new EncryptDecrypt(config?["Encryption:Key"] ?? string.Empty, config?["Encryption:IV"] ?? string.Empty);
                var keyData = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(apiRequest.KeyData));
                if (keyData == null)
                {
                    error = Messages.Common.Message.InvalidSession;
                    return false;
                }

                // Kept for the audit trail: the envelope has now been proved to belong to the
                // signed-in identity, so the IP / channel it reports is safe to record alongside
                // whatever change the request goes on to make (IAuditContextService reads it).
                context.HttpContext.Items[Services.AuditContextKeys.KeyData] = keyData;

                if (keyData.UserID != tokenUserId || (tokenUserTypeId > 0 && keyData.UserTypeID != tokenUserTypeId))
                {
                    new Logger().Write(new Exception(
                        $"KeyData/JWT identity mismatch. Token user={tokenUserId}/{tokenUserTypeId}, body user={keyData.UserID}/{keyData.UserTypeID}"));
                    error = Messages.Common.Message.Unauthorized;
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                error = Messages.Common.Message.InvalidSession;
                return false;
            }
        }

        private static JsonResult Unauthorized(string message) =>
            new(ResponseProcess.GetUnauthorizedResponse(message)) { StatusCode = 401 };

        private static JsonResult Forbidden(string message) =>
            new(ResponseProcess.GetUnauthorizedResponse(message)) { StatusCode = 403 };
    }

    // Role gate usable independently of JwtAuthorization (kept for flexibility). Uses numeric user-type ids.
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireUserTypeAttribute : ActionFilterAttribute
    {
        private readonly int[] _allowedUserTypeIds;

        public RequireUserTypeAttribute(params int[] allowedUserTypeIds)
        {
            _allowedUserTypeIds = allowedUserTypeIds ?? Array.Empty<int>();
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var userTypeId = context.HttpContext.Items["UserTypeId"] as int?;
            if (userTypeId == null || (_allowedUserTypeIds.Length > 0 && !_allowedUserTypeIds.Contains(userTypeId.Value)))
            {
                context.Result = new JsonResult(ResponseProcess.GetUnauthorizedResponse(Messages.Common.Message.NoPermission))
                {
                    StatusCode = 403
                };
                return;
            }

            await next();
        }
    }
}
