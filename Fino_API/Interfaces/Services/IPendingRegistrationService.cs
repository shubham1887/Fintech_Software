using FintechApp.Requests.OutsideRegistration;
using FintechApp.Responses.PendingRegistration;

namespace FintechApp.Interfaces.Services
{
    public interface IPendingRegistrationService
    {
        /// <summary>Sends a fresh OTP to an (as-yet unregistered) mobile for the public sign-up flow.</summary>
        Task<string> GenerateOtp(string mobile);

        /// <summary>Verifies the OTP for a mobile; on success the mobile is marked verified for sign-up.</summary>
        Task<string> VerifyOtp(string mobile, string otp);

        /// <summary>
        /// Stores a pending registration after enforcing: mobile OTP-verified, and no duplicate
        /// Mobile/Email/PAN/Aadhaar/GST in either existing users or the pending queue. Returns
        /// StatusType.Ok or a human-readable failure ("… already registered").
        /// </summary>
        Task<string> Register(OutsideRegisterRequest request);

        Task<List<PendingRegistrationResponse>> GetAll();
        Task<PendingRegistrationResponse?> GetByID(int id);

        /// <summary>Approve → create the real user, SMS a temporary password, link the row.</summary>
        Task<string> Approve(int id, int currentUserId, string ipAddress);

        Task<string> Reject(int id, string reason, int currentUserId);
    }
}
