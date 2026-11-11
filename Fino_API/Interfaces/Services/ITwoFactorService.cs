using FintechApp.Requests.TwoFactor;
using FintechApp.Responses.TwoFactor;

namespace FintechApp.Interfaces.Services
{
    public interface ITwoFactorService
    {
        Task<TwoFactorStatusResponse> Status(int userId);

        /// <summary>
        /// Begin enrolment: mint a secret and return the QR payload. Refused when two-step
        /// verification is already on — turning it off is an explicit, verified act, not a side
        /// effect of re-opening the setup screen.
        /// </summary>
        Task<(string failureMessage, TwoFactorSetupResponse? data)> BeginSetup(int userId);

        /// <summary>
        /// Finish enrolment by proving the app is generating matching codes, then hand back the
        /// recovery codes. Only here does the account actually start requiring a second step.
        /// </summary>
        Task<(string failureMessage, TwoFactorConfirmedResponse? data)> Confirm(int userId, ConfirmTwoFactorRequest request);

        Task<string> Disable(int userId, DisableTwoFactorRequest request);

        /// <summary>
        /// The sign-in check. Accepts an authenticator code or an unused recovery code; a spent
        /// recovery code and a replayed authenticator code are both refused.
        /// </summary>
        Task<bool> VerifyForLogin(int userId, string code);

        /// <summary>
        /// Clear an enrolment administratively — the "lost the phone" path. Removes the secret as
        /// well as the flag, so the old authenticator entry cannot be reused if it is ever enabled
        /// again.
        /// </summary>
        Task<string> ClearForUser(int userId);
    }
}
