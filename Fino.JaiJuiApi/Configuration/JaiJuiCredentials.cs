namespace Fino.JaiJuiApi.Configuration
{
    // Per-call credential override. When a caller (e.g. the DB-driven DMT router) already holds the
    // provider credentials for the active gateway row, it passes them here and the service uses them
    // instead of the static "JaiJuiApi" configuration section. Any blank field falls back to config.
    public class JaiJuiCredentials
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;   // registered mobile no (KeyData header)
        public string ApiToken { get; set; } = string.Empty; // API token           (KeyData header)
        public string AuthCode { get; set; } = string.Empty; // auth code           (request body)
    }
}
