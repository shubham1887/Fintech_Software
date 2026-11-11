using FintechApp.Requests.Login;
using FintechApp.Requests.TwoFactor;
using FintechApp.Responses.Login;

namespace FintechApp.Interfaces.Services
{
    public interface IAuthService
    {
        /// <summary>
        /// Sign in with mobile + password.
        ///
        /// Returns no token when the account has an authenticator enrolled: the response carries
        /// IsTwoFA and nothing else worth having, and the caller must finish through
        /// <see cref="LoginWithTwoFactor"/>. A null response means the credentials were rejected.
        /// </summary>
        Task<(LoginResponse?, TokenResponse?)> Login(LoginRequest request);

        /// <summary>Finish a two-step sign-in with an authenticator code or a recovery code.</summary>
        Task<(LoginResponse?, TokenResponse?)> LoginWithTwoFactor(TwoFactorLoginRequest request);
        Task<bool> Logout(int userId);

        /// <summary>
        /// Continue as the other role this mobile number holds (StaticModels/LinkedProfilePolicy).
        /// The failure string is empty on success and carries the reason otherwise.
        /// </summary>
        Task<(LoginResponse?, TokenResponse?, string failure)> SwitchProfile(
            int currentUserID, int targetUserID, string deviceType, string ipAddress);

        /// <summary>Send a password-reset OTP to a registered mobile. Returns StatusType.Ok or a message.</summary>
        Task<string> ForgotPasswordGenerateOtp(string mobile);

        /// <summary>Verify the reset OTP; on success set a temporary password (force change) and SMS it.</summary>
        Task<string> ForgotPasswordVerifyOtp(string mobile, string otp);
    }
}
