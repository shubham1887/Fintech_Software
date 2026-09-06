namespace FintechApp.Responses.Notification
{
    public class NotificationResponse
    {
        public int NotificationID { get; set; }
        public string NotificationText { get; set; } = string.Empty;
        public string NotificationImagePath { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
    }
}
