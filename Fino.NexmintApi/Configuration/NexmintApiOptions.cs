namespace Fino.NexmintApi.Configuration
{
    // Bound from the "NexmintApi" configuration section so the base URL and timeout can change
    // without recompiling. The credentials themselves (client_id / client_secret) are NOT kept here —
    // they belong to one "Add API" provider row, and arrive as a NexmintCredentials override.
    public class NexmintApiOptions
    {
        public const string SectionName = "NexmintApi";

        // Live gateway root. The sandbox is the same host with "/sandbox" appended, which is what
        // UseSandbox does — so switching a client to test money is one checkbox, not a new URL.
        public string BaseUrl { get; set; } = "https://api.nexmint.in/api/v1/gateway";

        public string ClientID { get; set; } = string.Empty;     // client-id header
        public string ClientSecret { get; set; } = string.Empty; // JWT signing key (HS256)
        public string WebhookSecret { get; set; } = string.Empty; // signs the payout callback JWT

        // true -> every call goes to {BaseUrl}/sandbox/... with Nexmint's fixed test outcomes.
        public bool UseSandbox { get; set; }

        public int TimeoutSeconds { get; set; } = 60;
    }
}
