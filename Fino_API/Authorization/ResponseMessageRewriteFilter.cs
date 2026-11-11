using FintechApp.Interfaces.Services;
using FintechApp.Responses;
using FintechApp.Responses.ApiUser;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FintechApp.Authorization
{
    /// <summary>
    /// The last thing that happens to a message before it leaves the server: the admin's rewrite
    /// rules (Settings → Response Messages) are applied to it.
    ///
    /// It lives here, as one filter, rather than at each of the hundred places a message is built,
    /// because that is the only way "whatever comes back, we send our own words instead" can
    /// actually be true. Every panel and app endpoint answers with <see cref="APIResponse"/> and
    /// every B2B endpoint with an <see cref="ApiUserEnvelope"/>; both carry exactly one
    /// human-readable field, and that is the field this rewrites.
    ///
    /// What it deliberately does NOT touch:
    ///   • the encrypted Data payload — report rows and balances are data, not prose, and a rule
    ///     firing inside them would corrupt what the screen parses;
    ///   • the provider log, the transaction description or anything else already stored;
    ///   • the rule-management screen itself, so an admin testing "Low balance" is not shown their
    ///     own rewrite of the confirmation they just received.
    ///
    /// Provider-specific rules do not fire here (the provider that answered is not knowable from a
    /// finished response) — those are applied at the gateway, where the provider IS known, and this
    /// filter then sees the already-rewritten text. Rules left at "All providers" fire everywhere.
    /// </summary>
    public class ResponseMessageRewriteFilter : IAsyncResultFilter
    {
        /// <summary>The admin's own rule screen is exempt — see the class comment.</summary>
        private const string ExemptController = "ResponseMessageRule";

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            try
            {
                if (context.Result is ObjectResult objectResult && objectResult.Value != null)
                {
                    var descriptor = context.ActionDescriptor as ControllerActionDescriptor;
                    bool exempt = descriptor != null &&
                        string.Equals(descriptor.ControllerName, ExemptController, StringComparison.OrdinalIgnoreCase);

                    if (!exempt)
                    {
                        var service = context.HttpContext.RequestServices.GetService<IResponseMessageRuleService>();
                        if (service != null)
                        {
                            string channel = IsSignedApi(context.HttpContext.Request.Path)
                                ? MessageChannel.Api
                                : MessageChannel.Panel;

                            switch (objectResult.Value)
                            {
                                case APIResponse api when api.Message.Length > 0:
                                    api.Message = await service.Apply(api.Message, 0, 0, channel);
                                    break;
                                case ApiUserEnvelope envelope when envelope.Message.Length > 0:
                                    envelope.Message = await service.Apply(envelope.Message, 0, 0, channel);
                                    break;
                            }
                        }
                    }
                }
            }
            catch
            {
                // A rewrite must never cost a caller their reply. Anything unexpected here and the
                // response goes out exactly as the controller built it.
            }

            await next();
        }

        /// <summary>/api/v1/... is the signed B2B surface; everything else is the panel and app.</summary>
        private static bool IsSignedApi(PathString path) =>
            path.HasValue && path.Value!.StartsWith("/api/v1", StringComparison.OrdinalIgnoreCase);
    }
}
