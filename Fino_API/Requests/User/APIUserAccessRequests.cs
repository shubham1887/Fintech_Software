namespace FintechApp.Requests.User
{
    /// <summary>Confirm API-token regeneration with the OTP sent to the API user's mobile.</summary>
    public class TokenGenerationRequest
    {
        public string OTP { get; set; } = string.Empty;
    }

    /// <summary>Update the API user's IP-security toggle and whitelisted IP list (OTP-gated).</summary>
    public class UpdateSecureIPRequest
    {
        public bool IsIPSecure { get; set; }

        /// <summary>Allowed IPs, newline or comma separated.</summary>
        public string WhitelistedIPs { get; set; } = string.Empty;

        public string OTP { get; set; } = string.Empty;
    }
}
