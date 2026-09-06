namespace Fino.PlanApi.Configuration
{
    // Bound from the "PlanApi" configuration section so base URL and credentials can change
    // without recompiling.
    public class PlanApiOptions
    {
        public const string SectionName = "PlanApi";

        public string BaseUrl { get; set; } = "https://planapi.in/api/Mobile";
        public string MemberId { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; } = 60;
    }
}
