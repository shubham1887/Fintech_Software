using Project.Processing;

namespace Project.Response.PayoutBeneficiary
{
    public class BeneficiaryViewModel
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private string _AccountNo;

        public string AccountNo
        {
            get => _AccountNo.TrimIfNotNull();
            set => _AccountNo = value.TrimIfNotNull();
        }

        private string _IFSC;

        public string IFSC
        {
            get => _IFSC.TrimIfNotNull();
            set => _IFSC = value.TrimIfNotNull();
        }
        
        private string _BankName;

        public string BankName
        {
            get => _BankName.TrimIfNotNull();
            set => _BankName = value.TrimIfNotNull();
        }
        private string _Document;

        public string Document
        {
            get => _Document.TrimIfNotNull();
            set => _Document = value.TrimIfNotNull();
        }
        public bool IsOTPVerified { get; internal set; }

        public string IsOTPVerifiedStr
        {
            get => (IsOTPVerified) ? "Yes" : "No";
        }

        public bool IsNameVerified { get; internal set; }

        public string IsNameVerifiedStr
        {
            get => (IsNameVerified) ? "Yes" : "No";
        }

        public int BeneficiaryID { get; internal set; }
    }
}