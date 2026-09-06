using Project.Processing;

namespace Project.Response.Beneficiary
{
    public class ViewModel
    {
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

        private string _PaymentType;

        public string PaymentType
        {
            get => _PaymentType.TrimIfNotNull();
            set => _PaymentType = value.TrimIfNotNull();
        }

        private string _BankName;

        public string BankName
        {
            get => _BankName.ToTitleCase();
            set => _BankName = value.ToTitleCase();
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

        private string _BeneficiaryID;

        public string BeneficiaryID
        {
            get => _BeneficiaryID.TrimIfNotNull();
            set => _BeneficiaryID = value.TrimIfNotNull();
        }

        private string _DMTBeneficiaryID;

        public string DMTBeneficiaryID
        {
            get => _DMTBeneficiaryID.TrimIfNotNull();
            set => _DMTBeneficiaryID = value.TrimIfNotNull();
        }

        private string _SenderID;

        public string SenderID
        {
            get => _SenderID.TrimIfNotNull();
            set => _SenderID = value.TrimIfNotNull();
        }

        private string _BankID;

        public string BankID
        {
            get => _BankID.TrimIfNotNull();
            set => _BankID = value.TrimIfNotNull();
        }

        private bool _IsVerified;

        public bool IsVerified
        {
            get => _IsVerified;
            set => _IsVerified = value;
        }

        private string _Status;

        public string Status
        {
            get => _Status;
            set => _Status = value;
        }
    }
}