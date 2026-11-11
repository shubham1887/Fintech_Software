namespace FintechApp.StaticModels
{
    /// <summary>
    /// Per-install e-invoice (IRP) configuration, bound from appsettings "EInvoice".
    ///
    /// White-label by design: a reseller signs up with its own GSP (or NIC directly), fills these
    /// in after deployment, and the real gateway takes over from the not-configured default with no
    /// rebuild. An install that leaves <see cref="Enabled"/> false keeps the safe default that
    /// fails loudly, which is correct for a client not yet on e-invoicing.
    ///
    /// The endpoints and credential fields are the surface every GSP exposes in some form; the
    /// exact paths and header names differ per GSP, so they are configuration rather than code.
    /// </summary>
    public class EInvoiceSettings
    {
        public const string Section = "EInvoice";

        /// <summary>Master switch. False → the not-configured gateway stays wired in.</summary>
        public bool Enabled { get; set; }

        /// <summary>The GSP / IRP base URL, e.g. https://api.yourgsp.com. No trailing slash needed.</summary>
        public string BaseUrl { get; set; } = string.Empty;

        /// <summary>Path that exchanges credentials for a session token. Blank → no auth step
        /// (the static credential headers below are sent on every call instead).</summary>
        public string AuthPath { get; set; } = string.Empty;

        /// <summary>Path that registers one invoice and returns the IRN.</summary>
        public string GeneratePath { get; set; } = "/eicore/v1.03/Invoice";

        // Credentials. ClientId/ClientSecret identify the GSP account; Username/Password are the
        // GSTIN-level portal login; Gstin is the registration documents are filed under.
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string Gstin { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>How long a fetched auth token is reused before re-authenticating. NIC tokens
        /// last about six hours; a conservative default avoids an auth call per document.</summary>
        public int AuthTokenHours { get; set; } = 6;

        // ── Scheduling (Hangfire) ────────────────────────────────────────────────────────

        /// <summary>Whether the recurring IRN / alert jobs are registered at all.</summary>
        public bool ScheduleEnabled { get; set; } = true;

        /// <summary>How often the queue is worked. Only registered when the gateway is configured —
        /// there is no point processing a queue with no portal to send to.</summary>
        public int ProcessIntervalMinutes { get; set; } = 30;

        public int ProcessBatchSize { get; set; } = 100;

        /// <summary>When the daily deadline sweeps run. Cron, server local time. 7am keeps the
        /// morning's alerts ready before the desk starts.</summary>
        public string AlertCron { get; set; } = "0 7 * * *";

        /// <summary>How far ahead the deadline sweeps look.</summary>
        public int AlertDaysAhead { get; set; } = 7;

        /// <summary>Enough is set to actually reach a portal. The gateway reports this as
        /// IsConfigured, and the DI picks the real gateway over the not-configured default on it.</summary>
        public bool IsConfigured =>
            Enabled
            && !string.IsNullOrWhiteSpace(BaseUrl)
            && !string.IsNullOrWhiteSpace(Gstin)
            && !string.IsNullOrWhiteSpace(ClientId);
    }
}
