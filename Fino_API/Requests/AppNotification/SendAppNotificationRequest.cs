namespace FintechApp.Requests.AppNotification
{
    /// <summary>
    /// Compose one alert.
    ///
    /// The old form had a single "UserTypeID" whose blank/zero value meant *everyone*, which is
    /// how an alert meant for one distributor's retailers ended up on every phone on the
    /// platform. Audience is now explicit and the server still clips it to the sender's own
    /// downline, so a distributor physically cannot address another branch.
    /// </summary>
    public class SendAppNotificationRequest
    {
        public string Title { get; set; } = string.Empty;

        /// <summary>The message. "Description" is accepted too — see the legacy alias below.</summary>
        public string Body { get; set; } = string.Empty;

        /// <summary>Legacy field name used by the old App Alert form.</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>NotificationAudience: All | Role | Users | Downline. Blank is rejected.</summary>
        public string Audience { get; set; } = string.Empty;

        /// <summary>User-type ids when Audience = Role.</summary>
        public List<int> UserTypeIDs { get; set; } = new();

        /// <summary>Explicit recipients when Audience = Users.</summary>
        public List<int> UserIDs { get; set; } = new();

        /// <summary>NotificationKind: Info | Alert | Promo | Ticket | Fund | Txn | System.</summary>
        public string Kind { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;

        /// <summary>In-app path opened when the alert is tapped, e.g. "/s/TicketReport".</summary>
        public string ActionUrl { get; set; } = string.Empty;

        /// <summary>
        /// Deliver a copy to the sender as well.
        ///
        /// The sender is normally dropped from their own audience — nobody needs their own
        /// announcement. The cost of that was an operator who sent an alert, looked at their own
        /// bell, saw nothing, and concluded the feature was broken. With this on, the composer can
        /// show them exactly what the network received.
        /// </summary>
        public bool IncludeSelf { get; set; }

        /// <summary>
        /// Retire the alert from everyone's bell at this moment. Null = it stays until deleted.
        ///
        /// Takes precedence over <see cref="ExpiresInDays"/> when both are sent.
        /// </summary>
        public DateTime? ExpiresOn { get; set; }

        /// <summary>
        /// Shorthand for the composer: retire it this many days after it is sent. 0 = never.
        /// Almost every alert is time-bound ("maintenance tonight", "offer till Sunday"), and
        /// before this nothing retired one — a user's bell filled with notices that had stopped
        /// being true and the only cure was reading and dismissing each one.
        /// </summary>
        public int ExpiresInDays { get; set; }

        // ---- legacy aliases, so the previously shipped App Alert form keeps working ----

        /// <summary>Old single-role field. 0 was "all users"; now it must be paired with an Audience.</summary>
        public int UserTypeID { get; set; }

        public List<LegacyUserTypeItem> UserTypeList { get; set; } = new();
    }

    public class LegacyUserTypeItem
    {
        public int UserTypeID { get; set; }
    }
}
