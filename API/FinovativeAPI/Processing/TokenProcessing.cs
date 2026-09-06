using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Project.BAL;
using Project.Model;
using Project.Processing.SDK;
using Project.Requests;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace Project.Processing
{
    public class TokenProcessing
    {
        public bool JWTValidation(HttpRequest req, KeyDataModel keyDataModel, string controller, string action)
        {
            try
            {
                StringValues authToken;
                req.Headers.TryGetValue("Authorization", out authToken);
                string authHeader = authToken.First();

                authHeader = authHeader.Replace("Bearer", "").Trim();
                var jwtToken = new JwtSecurityToken(authHeader);

                if (jwtToken.ValidTo < DateTime.UtcNow)
                {
                    return false;
                }

                string enc_userID = jwtToken.Subject;

                string userID = TokenManager.DecryptJWT(enc_userID, jwtToken.Id);
                if (!userID.EqualsIgnoreCase(keyDataModel.UserID.ToString()))
                {
                    return false;
                }

                IEnumerable<Claim> claim = jwtToken.Claims;

                var roleClaim = claim
                    .Where(x => x.Type == ClaimTypes.Role)
                    .FirstOrDefault();

                string enc_userTypeID = roleClaim.Value.ToString();
                string userTypeID = TokenManager.DecryptJWT(enc_userTypeID, jwtToken.Id);
                bool isAuthorize;

                if (Convert.ToInt32(userTypeID) == (int)UserType.Support)
                {
                    isAuthorize = new SupportUserAccessDetailBal().SelectForIsAuthorize(controller, action, keyDataModel.UserID);
                }
                else
                {
                    isAuthorize = new AuthorizationBAL().Select(controller, action, Convert.ToInt32(userTypeID));
                }
                return isAuthorize;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return false;
            }
        }

        public bool SDKJWTValidation(HttpRequest req, SDKKeyDataModel keyDataModel, string controller, string action)
        {
            try
            {
                StringValues authToken;
                req.Headers.TryGetValue("Authorization", out authToken);
                string authHeader = authToken.First();

                authHeader = authHeader.Replace("Bearer", "").Trim();
                var jwtToken = new JwtSecurityToken(authHeader);

                if (jwtToken.ValidTo < DateTime.UtcNow)
                {
                    return false;
                }

                string enc_userID = jwtToken.Subject;
                var MobileClaim = jwtToken.Claims.SingleOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2009/09/identity/claims/actor");
                string Mobile = new TokenManagerRedirectUrl().DecryptJWT(MobileClaim?.Value ?? "", jwtToken.Id);

                if (!Mobile.EqualsIgnoreCase(keyDataModel.Mobile.ToString()))
                {
                    return false;
                }

                UserMaster user = new UserBal().SelectByMobileFromCache(Mobile);
                bool isAuthorize = new AuthorizationBAL().Select(controller, action, Convert.ToInt32(user.UserTypeID));

                return isAuthorize;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return false;
            }
        }
    }
}