using Project.Processing;

namespace Project.Response.TransactionMaster
{
    public class CallbackResponse
    {
        private int _TransactionMasterID;

        internal int TransactionMasterID
        {
            get => _TransactionMasterID;
            set => _TransactionMasterID = value;
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private string _ConsumerNumber;

        public string ConsumerNumber
        {
            get => _ConsumerNumber.TrimIfNotNull();
            set => _ConsumerNumber = value.TrimIfNotNull();
        }

        private string _ProviderRequestID;

        public string ProviderRequestID
        {
            get => _ProviderRequestID.TrimIfNotNull();
            set => _ProviderRequestID = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _Response;

        public string Response
        {
            get => _Response.TrimIfNotNull();
            set => _Response = value.TrimIfNotNull();
        }

        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private decimal _OpeningBalance;

        public decimal OpeningBalance
        {
            get => _OpeningBalance;
            set => _OpeningBalance = value;
        }

        private decimal _ClosingBalance;

        public decimal ClosingBalance
        {
            get => _ClosingBalance;
            set => _ClosingBalance = value;
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

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        public int UserTypeID { get; internal set; }
    }
}