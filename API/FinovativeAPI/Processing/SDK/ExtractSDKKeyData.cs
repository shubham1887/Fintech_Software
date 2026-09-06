using Project.Requests;
using System.Linq;

namespace Project.Processing.SDK
{
    public class ExtractSDKKeyData
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        internal SDKKeyDataModel ExtractKeyData(string ciperText)
        {
            string sdkJwtToken = ed.Decrypt(ciperText);
            var jwtToken = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(sdkJwtToken);

            SDKKeyDataModel sdkKeyDataModel = new SDKKeyDataModel();
            sdkKeyDataModel.RequestID = jwtToken?.Subject ?? "";

            var RoleClaim = jwtToken.Claims.SingleOrDefault(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
            sdkKeyDataModel.AgentCode = RoleClaim?.Value ?? "";

            var MobileClaim = jwtToken.Claims.SingleOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2009/09/identity/claims/actor");
            sdkKeyDataModel.Mobile = new TokenManagerRedirectUrl().DecryptJWT(MobileClaim?.Value ?? "", jwtToken.Id);

            return sdkKeyDataModel;
        }
    }
}