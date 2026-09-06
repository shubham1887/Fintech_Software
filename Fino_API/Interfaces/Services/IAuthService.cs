using FintechApp.Requests.Login;
using FintechApp.Responses.Login;

namespace FintechApp.Interfaces.Services
{
    public interface IAuthService
    {
        Task<(LoginResponse?, TokenResponse?)> Login(LoginRequest request);
        Task<bool> Logout(int userId);

        /// <summary>Send a password-reset OTP to a registered mobile. Returns StatusType.Ok or a message.</summary>
        Task<string> ForgotPasswordGenerateOtp(string mobile);

        /// <summary>Verify the reset OTP; on success set a temporary password (force change) and SMS it.</summary>
        Task<string> ForgotPasswordVerifyOtp(string mobile, string otp);
    }
}
