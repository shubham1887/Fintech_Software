using FintechApp.Requests.Credential;
using FintechApp.Responses.Login;

namespace FintechApp.Interfaces.Services
{
    public interface ICredentialService
    {
        Task<(string failureMessage, LoginResponse? response)> Login(LoginCredentialRequest request, string ipAddress);
        Task<(string failureMessage, LoginResponse? response)> VerifyTwoFAatLogin(VerifyTwoFARequest request, string ipAddress);
        Task<string> ForgotPassword(ForgotPasswordCredentialRequest request, string ipAddress);
        Task<string> ForgotPasswordGenerateOTP(ForgotPasswordCredentialRequest request, string ipAddress);
        Task<string> VerifyForgotPasswordOTP(ForgotPasswordOTPRequest request, string ipAddress);
        Task<(string failureMessage, LoginResponse? response)> NewDeviceLogin(NewDeviceLoginRequest request, string ipAddress);
        Task<(string failureMessage, LoginResponse? response)> RefreshJWTToken(int userId, string refreshToken);
        Task<string> InitialChangePassword(UpdatePasswordCredentialRequest request, int userId, string ipAddress);
        Task<bool> Logout(int userId);
        Task<string> ChangePasswordAfterExpiry(ChangePasswordAfterExpiryRequest request, string ipAddress);
        Task<(string failureMessage, FetchDetailsForPincodeResponse? response)> FetchDetailsForPincode(string pincode);
    }
}
