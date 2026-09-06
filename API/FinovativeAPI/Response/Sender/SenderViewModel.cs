using Project.Processing;

namespace Project.Response.Sender
{
    public class SenderViewModel
    {
        public bool IsActive { get; internal set; }
        public bool IsOTPVerified { get; internal set; }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private string _Mobile;

        public string Mobile
        {
            get => _Mobile.TrimIfNotNull();
            set => _Mobile = value.TrimIfNotNull();
        }

        private string _Address;

        public string Address
        {
            get => _Address.TrimIfNotNull();
            set => _Address = value.TrimIfNotNull();
        }

        private string _GUID;

        public string GUID
        {
            get => _GUID.TrimIfNotNull();
            set => _GUID = value.TrimIfNotNull();
        }

        private int _SenderID;

        public int SenderID
        {
            get => _SenderID;
            set => _SenderID = value;
        }

        private int _BeneficiaryCount;

        public int BeneficiaryCount
        {
            get => _BeneficiaryCount;
            set => _BeneficiaryCount = value;
        }

        private string _Pincode;

        public string Pincode
        {
            get => _Pincode.TrimIfNotNull();
            set => _Pincode = value.TrimIfNotNull();
        }
    }
}