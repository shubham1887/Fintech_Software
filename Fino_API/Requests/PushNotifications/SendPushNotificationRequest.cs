namespace FintechApp.Requests.PushNotifications
{
    public class SendPushNotificationRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<UserTypeItem> UserTypeList { get; set; } = new();
    }

    public class UserTypeItem
    {
        public int UserTypeID { get; set; }
    }
}
