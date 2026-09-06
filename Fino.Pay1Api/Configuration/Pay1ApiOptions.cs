namespace Fino.Pay1Api.Configuration
{
    // Bound from the "Pay1Api" configuration section so the base URL and timeout can change
    // without recompiling. Credentials are NOT expected here in normal operation: they live on
    // the provider row in "API Providers" and arrive per call as Pay1Credentials.
    public class Pay1ApiOptions
    {
        public const string SectionName = "Pay1Api";

        /// <summary>Host or full receiveApi path. "/apis/receiveApi" is appended when missing.</summary>
        public string BaseUrl { get; set; } = "https://recharges.pay1.in";

        /// <summary>Account id of the partner (Pay1 "partner_id", e.g. P0232050).</summary>
        public string PartnerId { get; set; } = string.Empty;

        /// <summary>API key. Never sent — it is only hashed into hash_code.</summary>
        public string ApiKey { get; set; } = string.Empty;

        public int TimeoutSeconds { get; set; } = 60;
    }
}
