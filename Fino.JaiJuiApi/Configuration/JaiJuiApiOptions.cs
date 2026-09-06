namespace Fino.JaiJuiApi.Configuration
{
    // Bound from the "JaiJuiApi" configuration section so base URL and credentials can change
    // without recompiling. Mobile + ApiToken are sent in the "KeyData" header; AuthCode is sent
    // in every request body (per the JaiJui DMT specification).
    public class JaiJuiApiOptions
    {
        public const string SectionName = "JaiJuiApi";

        public string BaseUrl { get; set; } = "https://api.jaijuirecharge.in/DMT/v1";
        public string Mobile { get; set; } = string.Empty;   // registered mobile no (KeyData header)
        public string ApiToken { get; set; } = string.Empty; // API token           (KeyData header)
        public string AuthCode { get; set; } = string.Empty; // auth code           (request body)

        // Balance enquiry endpoint. JaiJui's balance API is not part of the DMT transfer spec, so it is
        // configured separately: set BalanceUrl to the absolute endpoint, OR leave it blank to use
        // {BaseUrl}/{BalancePath}. Update these once the exact JaiJui balance URL is known — no code change.
        public string BalanceUrl { get; set; } = string.Empty;
        public string BalancePath { get; set; } = "GetBalance";

        public int TimeoutSeconds { get; set; } = 60;
    }
}
