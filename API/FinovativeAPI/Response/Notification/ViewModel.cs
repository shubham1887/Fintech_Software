using Project.Processing;

namespace Project.Response.Notification
{
    public class ViewModel
    {
        private int _NotificationID;

        public int NotificationID
        {
            get => _NotificationID;
            set => _NotificationID = value;
        }

        private string _NotificationText;

        public string NotificationText
        {
            get => _NotificationText.TrimIfNotNull();
            set => _NotificationText = value.TrimIfNotNull();
        }
    }
}