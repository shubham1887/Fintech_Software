using FintechApp.Entities;
using FintechApp.StaticModels;

namespace FintechApp.Interfaces.Services
{
    /// <summary>Outcome of authenticating a B2B API call. Carries a machine code, never a bare bool.</summary>
    public sealed class ApiUserAuthResult
    {
        public UserMaster? User { get; init; }
        public string ErrorCode { get; init; } = ApiUserErrorCode.None;
        public string Message { get; init; } = string.Empty;

        public bool Succeeded => User != null && ErrorCode.Length == 0;

        public static ApiUserAuthResult Ok(UserMaster user) => new() { User = user };
        public static ApiUserAuthResult Fail(string errorCode, string message) =>
            new() { ErrorCode = errorCode, Message = message };
    }

    /// <summary>
    /// Credential gate for every /api/v1 endpoint: userid + API token, API-user role, active
    /// account, and the optional IP whitelist configured on the API Access page.
    ///
    /// Centralised so all endpoints authenticate identically and return identical error codes —
    /// an integrator debugging a 'why am I rejected' never sees two different wordings for the
    /// same cause.
    /// </summary>
    public interface IApiUserAuthService
    {
        Task<ApiUserAuthResult> Authenticate(string rawUserId, string token, string clientIpAddress);
    }
}
