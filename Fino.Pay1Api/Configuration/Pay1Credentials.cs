namespace Fino.Pay1Api.Configuration
{
    // Per-call credential override. The caller (the DB-driven provider gateway) already holds the
    // credentials of the provider row that is serving the operator, and passes them here instead of
    // relying on the static "Pay1Api" configuration section. Any blank field falls back to config.
    public class Pay1Credentials
    {
        public string BaseUrl { get; set; } = string.Empty;    // host or full receiveApi path
        public string PartnerId { get; set; } = string.Empty;  // partner_id / account id (P0232050)
        public string ApiKey { get; set; } = string.Empty;     // API key — hashed, never transmitted
    }
}
