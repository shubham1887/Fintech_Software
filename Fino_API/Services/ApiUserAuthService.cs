using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    /// <inheritdoc cref="IApiUserAuthService"/>
    public class ApiUserAuthService : IApiUserAuthService
    {
        private readonly IUserRepository _userRepo;

        public ApiUserAuthService(IUserRepository userRepo) => _userRepo = userRepo;

        public async Task<ApiUserAuthResult> Authenticate(string rawUserId, string token, string clientIpAddress)
        {
            if (string.IsNullOrWhiteSpace(rawUserId))
                return ApiUserAuthResult.Fail(ApiUserErrorCode.UserIdRequired, "Parameter 'userid' is required.");
            if (!int.TryParse(rawUserId.Trim(), out int userId) || userId <= 0)
                return ApiUserAuthResult.Fail(ApiUserErrorCode.UserIdRequired, "Parameter 'userid' must be a positive number.");
            if (string.IsNullOrWhiteSpace(token))
                return ApiUserAuthResult.Fail(ApiUserErrorCode.TokenRequired, "Parameter 'token' is required.");

            var user = await _userRepo.GetUserById(userId);
            if (user == null || user.IsDeleted)
                return ApiUserAuthResult.Fail(ApiUserErrorCode.UserNotFound, "Invalid userid.");
            if (user.UserTypeID != UserTypeCode.ApiUser)
                return ApiUserAuthResult.Fail(ApiUserErrorCode.NotApiUser, "This API is available to API users only.");
            if (!user.IsActive)
                return ApiUserAuthResult.Fail(ApiUserErrorCode.AccountInactive, "Account is inactive.");

            // Fixed-length ordinal compare — the token is a random opaque string, never trimmed
            // or case-folded, so a copy/paste with different casing is a genuine mismatch.
            if (!string.Equals(user.APIToken ?? string.Empty, token.Trim(), StringComparison.Ordinal))
                return ApiUserAuthResult.Fail(ApiUserErrorCode.InvalidToken, "Invalid token.");

            if (user.IsIPSecure)
            {
                string ip = (clientIpAddress ?? string.Empty).Trim();
                var allowed = (user.WhitelistedIPs ?? string.Empty)
                    .Split(new[] { ',', '\n', '\r', ';', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim());
                if (!allowed.Contains(ip, StringComparer.OrdinalIgnoreCase))
                    return ApiUserAuthResult.Fail(ApiUserErrorCode.IpNotWhitelisted, $"IP {ip} is not whitelisted.");
            }

            return ApiUserAuthResult.Ok(user);
        }
    }
}
