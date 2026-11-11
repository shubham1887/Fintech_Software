namespace FintechApp.Responses.LoginHistory
{
    public class LoginHistoryResponse
    {
        public int LoginHistoryID { get; set; }
        public int UserID { get; set; }

        // ── Who signed in ──────────────────────────────────────────────────────────────────
        // Resolved here rather than joined in the browser. The screen used to look the name up in
        // its own Users/Select call, which is capped at 500 active users and is not reachable by a
        // retailer at all — so a login by anyone deactivated, deleted, or beyond the cap read as
        // "User 42", and the one report whose first column is a person had no person on it.
        public string UserName { get; set; } = string.Empty;
        public string FirmName { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        /// <summary>The auto-generated User Code (AliasName) — RD000001 and so on.</summary>
        public string UserCode { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;

        public string IPAddress { get; set; } = string.Empty;

        // Token is deliberately NOT returned. It is the session's JWT id, and a report that any
        // upline can open is not the place for it; nothing reads it, and a report row carrying a
        // session identifier is a finding waiting to be written up.

        public string Through { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string LoginMessage { get; set; } = string.Empty;

        /// <summary>Where the sign-in happened, as text — an address if the client resolved one.</summary>
        public string Location { get; set; } = string.Empty;
        public string Latitude { get; set; } = string.Empty;
        public string Longitude { get; set; } = string.Empty;

        public string SimCardDetails { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
