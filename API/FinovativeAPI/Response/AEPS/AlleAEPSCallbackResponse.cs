using Project.Processing;

namespace Project.Response.AEPS
{
    public class AlleAEPSCallbackResponse
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

        private string _BankRRN;

        public string BankRRN
        {
            get => _BankRRN.TrimIfNotNull();
            set => _BankRRN = value.TrimIfNotNull();
        }

        private string _AvailableBalance;

        public string AvailableBalance
        {
            get => _AvailableBalance.TrimIfNotNull();
            set => _AvailableBalance = value.TrimIfNotNull();
        }

        private string _BankName;

        public string BankName
        {
            get => _BankName.TrimIfNotNull();
            set => _BankName = value.TrimIfNotNull();
        }

        private string _APIUserRequestID;

        public string APIUserRequestID
        {
            get => _APIUserRequestID.TrimIfNotNull();
            set => _APIUserRequestID = value.TrimIfNotNull();
        }

        private string _ReferenceNo;

        public string ReferenceNo
        {
            get => _ReferenceNo.TrimIfNotNull();
            set => _ReferenceNo = value.TrimIfNotNull();
        }

        private string _Amount;

        public string Amount
        {
            get => _Amount.TrimIfNotNull();
            set => _Amount = value.TrimIfNotNull();
        }

        private int _VendorID;

        public int VendorID
        {
            get => _VendorID;
            set => _VendorID = value;
        }
    }
}