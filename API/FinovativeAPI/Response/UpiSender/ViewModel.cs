using Project.Processing;

namespace Project.Response.UpiSender
{
    public class ViewModel
    {
        private string _Name;

        public string Name
        {
            get => _Name.ToTitleCase();
            set => _Name = value.ToTitleCase();
        }

        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private string _Mobile;

        public string Mobile
        {
            get => _Mobile.TrimIfNotNull();
            set => _Mobile = value.TrimIfNotNull();
        }

        private string _UpiSenderID;

        public string UpiSenderID
        {
            get => _UpiSenderID.TrimIfNotNull();
            set => _UpiSenderID = value.TrimIfNotNull();
        }

        private string _Address;

        public string Address
        {
            get => _Address.ToTitleCase();
            set => _Address = value.ToTitleCase();
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

        private string _State;

        public string State
        {
            get => _State.TrimIfNotNull();
            set => _State = value.TrimIfNotNull();
        }
    }
}