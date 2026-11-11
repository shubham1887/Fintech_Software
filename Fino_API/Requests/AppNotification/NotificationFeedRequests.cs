namespace FintechApp.Requests.AppNotification
{
    /// <summary>The bell's read. The caller is always the signed-in user — never a parameter.</summary>
    public class GetNotificationFeedRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 30;
        public bool UnreadOnly { get; set; }
        /// <summary>Optional NotificationKind filter.</summary>
        public string Kind { get; set; } = string.Empty;
    }

    public class MarkNotificationReadRequest
    {
        /// <summary>The alert to mark. 0 with <see cref="All"/> marks the whole feed.</summary>
        public int AppNotificationID { get; set; }
        public bool All { get; set; }
    }

    /// <summary>Admin's "what did we send" list.</summary>
    public class GetSentNotificationsRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    /// <summary>Register (or refresh) the FCM token of the device making the call.</summary>
    public class RegisterDeviceRequest
    {
        public string FcmToken { get; set; } = string.Empty;
        /// <summary>android | web | ios. Blank falls back to the KeyData DeviceType.</summary>
        public string Platform { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string AppVersion { get; set; } = string.Empty;
        /// <summary>Sign-out: drop this token so the next user of the handset is not pushed to.</summary>
        public bool Remove { get; set; }
    }
}
