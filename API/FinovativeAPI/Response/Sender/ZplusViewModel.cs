using Project.Processing;

namespace Project.Response.Sender
{
    public class ZplusViewModel
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

        private string _SenderID;

        public string SenderID
        {
            get => _SenderID.TrimIfNotNull();
            set => _SenderID = value.TrimIfNotNull();
        }

        private string _Address;

        public string Address
        {
            get => _Address.ToTitleCase();
            set => _Address = value.ToTitleCase();
        }

        private string _Kycflag;

        public string Kycflag
        {
            get => _Kycflag.ToTitleCase();
            set => _Kycflag = value.ToTitleCase();
        }

        private decimal _RemainingNEFTLimit;

        public decimal RemainingNEFTLimit
        {
            get => _RemainingNEFTLimit;
            set => _RemainingNEFTLimit = value;
        }

        private decimal _RemainingIMPSLimit;

        public decimal RemainingIMPSLimit
        {
            get => _RemainingIMPSLimit;
            set => _RemainingIMPSLimit = value;
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