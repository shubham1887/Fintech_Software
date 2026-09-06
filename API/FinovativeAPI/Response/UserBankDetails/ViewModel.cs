using Project.Processing;

namespace Project.Response.UserBankDetails
{
    public class ViewModel
    {
        private int _UserBankDetailsID;
        private int _UserID;
        private string _BankName;
        private string _AccountNumber;
        private string _IFSC;
        private string _UPINumber;
        private string _QRCodeImageURL;

        public int UserBankDetailsID
        {
            get => _UserBankDetailsID;
            set => _UserBankDetailsID = value;
        }

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        public string BankName
        {
            get => _BankName.TrimIfNotNull();
            set => _BankName = value.TrimIfNotNull();
        }

        public string AccountNumber
        {
            get => _AccountNumber.TrimIfNotNull();
            set => _AccountNumber = value.TrimIfNotNull();
        }

        public string IFSC
        {
            get => _IFSC.TrimIfNotNull();
            set => _IFSC = value.TrimIfNotNull();
        }

        private string _HolderName;

        public string HolderName
        {
            get => _HolderName.TrimIfNotNull();
            set => _HolderName = value.TrimIfNotNull();
        }

        private string _Branch;

        public string Branch
        {
            get => _Branch.TrimIfNotNull();
            set => _Branch = value.TrimIfNotNull();
        }

        public string UPINumber
        {
            get => _UPINumber.TrimIfNotNull();
            set => _UPINumber = value.TrimIfNotNull();
        }

        public string QRCodeImageURL
        {
            get => _QRCodeImageURL.TrimIfNotNull();
            set => _QRCodeImageURL = value.TrimIfNotNull();
        }

        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private string _Role;

        public string Role
        {
            get => _Role.TrimIfNotNull();
            set => _Role = value.TrimIfNotNull();
        }
    }
}