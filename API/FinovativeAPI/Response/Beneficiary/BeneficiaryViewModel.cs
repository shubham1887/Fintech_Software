using Project.Processing;

namespace Project.Response.Beneficiary
{
    public class BeneficiaryViewModel
    {
        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private string _Sender;

        public string Sender
        {
            get => _Sender.TrimIfNotNull();
            set => _Sender = value.TrimIfNotNull();
        }

        private string _SenderMobile;

        public string SenderMobile
        {
            get => _SenderMobile.TrimIfNotNull();
            set => _SenderMobile = value.TrimIfNotNull();
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

        private string _IFSC;

        public string IFSC
        {
            get => _IFSC.TrimIfNotNull();
            set => _IFSC = value.TrimIfNotNull();
        }

        private string _AccountNo;

        public string AccountNo
        {
            get => _AccountNo.TrimIfNotNull();
            set => _AccountNo = value.TrimIfNotNull();
        }

        public int BeneficiaryID { get; internal set; }

        public bool IsActive { get; internal set; }
        public bool IsNameVerified { get; internal set; }

        public string IsNameVerifiedStr
        {
            get => (IsNameVerified) ? "Yes" : "No";
        }

        public bool IsOTPVerified { get; internal set; }

        public string IsOTPVerifiedStr
        {
            get => (IsOTPVerified) ? "Yes" : "No";
        }
    }
}