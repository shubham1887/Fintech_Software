namespace Fino.NexmintApi.Configuration
{
    // Per-call credential override. When a caller (e.g. the DB-driven payout router) already holds the
    // provider credentials for the active gateway row, it passes them here and the service uses them
    // instead of the static "NexmintApi" configuration section. Any blank field falls back to config.
    public class NexmintCredentials
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string ClientID { get; set; } = string.Empty;     // client-id header
        public string ClientSecret { get; set; } = string.Empty; // JWT signing key (HS256)
        public string WebhookSecret { get; set; } = string.Empty; // verifies the payout callback JWT

        // Tri-state on purpose: null means "the config section decides", so a provider row that
        // says nothing about sandbox does not silently force live money onto a test install.
        public bool? UseSandbox { get; set; }
    }
}
