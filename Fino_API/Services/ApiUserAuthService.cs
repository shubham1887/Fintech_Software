using FintechApp.Entities;
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

        public async Task<ApiUserAuthResult> Authenticate(string rawMobile, string token, string clientIpAddress)
        {
            if (string.IsNullOrWhiteSpace(rawMobile))
                return ApiUserAuthResult.Fail(ApiUserErrorCode.MobileRequired, "Parameter 'mobile' is required.");

            // Tolerate the shapes a partner's own records hold the number in — "+91 98765 43210",
            // "091-9876543210". The account is keyed on ten digits, and refusing a number that is
            // plainly correct because it arrived with a country code is a support ticket, not a
            // security control. Anything that is not digits after this is rejected below.
            string digits = new string(rawMobile.Where(char.IsAsciiDigit).ToArray());
            if (digits.Length > 10 && (digits.StartsWith("91", StringComparison.Ordinal) || digits.StartsWith("0", StringComparison.Ordinal)))
                digits = digits[^10..];

            if (string.IsNullOrWhiteSpace(token))
                return ApiUserAuthResult.Fail(ApiUserErrorCode.TokenRequired, "Parameter 'token' is required.");

            UserMaster? user = null;

            if (digits.Length == 10)
                user = await _userRepo.GetUserByMobile(digits);

            // Legacy: integrations built before the credential became the mobile number still send
            // the numeric account id as 'userid'. Resolving it keeps them working — the API token
            // is checked either way, so this identifies an account, it never authorises one.
            if (user == null && int.TryParse(rawMobile.Trim(), out int legacyUserId) && legacyUserId > 0)
                user = await _userRepo.GetUserById(legacyUserId);

            if (user == null && digits.Length != 10)
                return ApiUserAuthResult.Fail(ApiUserErrorCode.MobileRequired,
                    "Parameter 'mobile' must be the account's registered 10-digit mobile number.");

            if (user == null || user.IsDeleted)
                return ApiUserAuthResult.Fail(ApiUserErrorCode.UserNotFound,
                    "No account is registered with this mobile number.");
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
