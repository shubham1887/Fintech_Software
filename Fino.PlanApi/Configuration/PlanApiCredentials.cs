namespace Fino.PlanApi.Configuration
{
    /// <summary>
    /// Per-call credential override, so a host can keep the provider's account on its own
    /// "API Providers" row instead of appsettings and let an admin rotate it without a redeploy.
    /// Any field left blank falls back to the "PlanApi" configuration section, which means a host
    /// that passes nothing keeps the original configuration-only behaviour.
    /// </summary>
    public class PlanApiCredentials
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string MemberId { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
