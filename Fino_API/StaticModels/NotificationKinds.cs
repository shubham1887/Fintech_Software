namespace FintechApp.StaticModels
{
    /// <summary>
    /// What an alert is about. Drives the icon and tone in the bell feed, and the Android
    /// notification channel the push lands in — so a user can silence promotions without
    /// silencing "your wallet was debited".
    /// </summary>
    public static class NotificationKind
    {
        public const string Info = "Info";
        public const string Alert = "Alert";
        public const string Promo = "Promo";
        public const string Ticket = "Ticket";
        public const string Fund = "Fund";
        public const string Txn = "Txn";
        public const string System = "System";

        private static readonly string[] All = { Info, Alert, Promo, Ticket, Fund, Txn, System };

        public static string Normalise(string? kind) =>
            All.FirstOrDefault(k => k.Equals(kind?.Trim(), StringComparison.OrdinalIgnoreCase)) ?? Info;

        /// <summary>
        /// Android notification channel for a kind. Channels are created by the app on first
        /// launch; an unknown id would make Android drop the notification silently on 8.0+.
        /// </summary>
        public static string ChannelFor(string kind) => Normalise(kind) switch
        {
            Promo => "fino_promo",
            Txn or Fund => "fino_txn",
            _ => "fino_alerts",
        };
    }

    /// <summary>Who an alert is for. Resolved server-side against the sender's own downline.</summary>
    public static class NotificationAudience
    {
        /// <summary>Everyone the sender is allowed to reach.</summary>
        public const string All = "All";
        /// <summary>Whole user-types, e.g. every retailer.</summary>
        public const string Role = "Role";
        /// <summary>A hand-picked list of user ids.</summary>
        public const string Users = "Users";
        /// <summary>The sender's own downline only.</summary>
        public const string Downline = "Downline";

        private static readonly string[] Allowed = { All, Role, Users, Downline };

        public static string Normalise(string? audience) =>
            Allowed.FirstOrDefault(a => a.Equals(audience?.Trim(), StringComparison.OrdinalIgnoreCase)) ?? Role;
    }
}
