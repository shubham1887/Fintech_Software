namespace FintechApp.Responses.User
{
    public class APITokenResponse
    {
        public string APIToken { get; set; } = string.Empty;
    }

    public class SecureIPResponse
    {
        public bool IsIPSecure { get; set; }

        /// <summary>Allowed IPs, newline or comma separated.</summary>
        public string WhitelistedIPs { get; set; } = string.Empty;
    }
}
