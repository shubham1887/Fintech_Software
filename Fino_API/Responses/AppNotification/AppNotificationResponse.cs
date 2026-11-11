namespace FintechApp.Responses.AppNotification
{
    /// <summary>One row in the bell feed.</summary>
    public class AppNotificationResponse
    {
        public int AppNotificationID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Kind { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string ActionUrl { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime DateTime { get; set; }
    }

    /// <summary>What the bell needs in one call: the badge count and the first page.</summary>
    public class NotificationFeedResponse
    {
        public int UnreadCount { get; set; }
        public int TotalCount { get; set; }
        public List<AppNotificationResponse> Items { get; set; } = new();
    }

    /// <summary>Admin's sent-alert history — audience and reach, not just the text.</summary>
    public class SentNotificationResponse
    {
        public int AppNotificationID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Kind { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string AudienceRoles { get; set; } = string.Empty;
        public int RecipientCount { get; set; }
        public int PushedCount { get; set; }
        public int ReadCount { get; set; }
        public int CreatedByUserID { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        /// <summary>When it drops out of recipients' bells. Null = it stays until deleted.</summary>
        public DateTime? ExpiresOn { get; set; }
        /// <summary>Already retired — the ledger badges these so "nobody can see it" is visible.</summary>
        public bool IsExpired { get; set; }
    }

    /// <summary>The outcome of composing one alert, so the sender sees the real reach.</summary>
    public class SendNotificationResult
    {
        public int AppNotificationID { get; set; }
        public int RecipientCount { get; set; }
        /// <summary>Recipients with at least one registered device. The rest read it in the bell.</summary>
        public int DeviceCount { get; set; }
        /// <summary>False when no FCM service account is configured on this installation.</summary>
        public bool PushConfigured { get; set; }
    }

    /// <summary>
    /// Whether this install can reach a phone's notification tray, so the alert screens can say
    /// so before somebody sends a broadcast and wonders why no phone buzzed.
    /// </summary>
    public class PushStatusResponse
    {
        /// <summary>A Firebase service account is configured — the server can send.</summary>
        public bool PushConfigured { get; set; }

        /// <summary>Active device registrations across the platform (phones + browsers).</summary>
        public int RegisteredDevices { get; set; }

        /// <summary>Of those, Android handsets.</summary>
        public int AndroidDevices { get; set; }

        /// <summary>
        /// Plain-language reading of the two facts above — what to fix, in the order it has to
        /// be fixed. Empty when push is working.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
