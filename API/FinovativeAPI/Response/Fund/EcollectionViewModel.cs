using Project.Processing;

namespace Project.Response.Fund
{
    public class EcollectionViewModel
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private string _DOC;

        public string DOC
        {
            get => _DOC.TrimIfNotNull();
            set => _DOC = value.TrimIfNotNull();
        }

        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private string _WalletName;

        public string WalletName
        {
            get => _WalletName.TrimIfNotNull();
            set => _WalletName = value.TrimIfNotNull();
        }

        private string _VirtualACCode;

        public string VirtualACCode
        {
            get => _VirtualACCode.TrimIfNotNull();
            set => _VirtualACCode = value.TrimIfNotNull();
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private string _UTR;

        public string UTR
        {
            get => _UTR.TrimIfNotNull();
            set => _UTR = value.TrimIfNotNull();
        }

        private string _PayeeAccountNumber;

        public string PayeeAccountNumber
        {
            get => _PayeeAccountNumber.TrimIfNotNull();
            set => _PayeeAccountNumber = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _Reason;

        public string Reason
        {
            get => _Reason.TrimIfNotNull();
            set => _Reason = value.TrimIfNotNull();
        }
    }
}