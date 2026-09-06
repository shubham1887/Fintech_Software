using Project.Processing;

namespace Project.Response.LoginHistory
{
    public class LoginHistoryViewModel
    {
        public int UserID { get; internal set; }

        private string _Through;

        public string Through
        {
            get => _Through.TrimIfNotNull();
            set => _Through = value.TrimIfNotNull();
        }

        private string _LoginMessage;

        public string LoginMessage
        {
            get => _LoginMessage.TrimIfNotNull();
            set => _LoginMessage = value.TrimIfNotNull();
        }

        private string _Date;

        public string Date
        {
            get => _Date.TrimIfNotNull();
            set => _Date = value.TrimIfNotNull();
        }

        private string _IPAddress;

        public string IPAddress
        {
            get => _IPAddress.TrimIfNotNull();
            set => _IPAddress = value.TrimIfNotNull();
        }

        private string _User;

        public string User
        {
            get => _User.TrimIfNotNull();
            set => _User = value.TrimIfNotNull();
        }

        private string _UserType;

        public string UserType
        {
            get => _UserType.TrimIfNotNull();
            set => _UserType = value.TrimIfNotNull();
        }
    }
}