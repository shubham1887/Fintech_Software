using Project.Processing;

namespace Project.Response.MyNetLinkAPIResponse
{
    public class SelectSenderAPIResponse
    {
        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _Name;

        public string Name
        {
            get => _Name.ToTitleCase();
            set => _Name = value.ToTitleCase();
        }

        private string _Mobile;

        public string Mobile
        {
            get => _Mobile.TrimIfNotNull();
            set => _Mobile = value.TrimIfNotNull();
        }

        private string _SenderID;

        public string SenderID
        {
            get => _SenderID.TrimIfNotNull();
            set => _SenderID = value.TrimIfNotNull();
        }

        private int _RemainingLimit;

        public int RemainingLimit
        {
            get => _RemainingLimit;
            set => _RemainingLimit = value;
        }

        private int _Consumed;

        public int Consumed
        {
            get => _Consumed;
            set => _Consumed = value;
        }
    }
}