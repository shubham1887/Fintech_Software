using Project.Processing;

namespace Project.Response.TransactionMaster
{
    public class TransactionMasterViewModel
    {
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

        private string _TranactionID;

        public string TranactionID
        {
            get => _TranactionID.TrimIfNotNull();
            set => _TranactionID = value.TrimIfNotNull();
        }

        private decimal _ProviderOpeningBalance;
        public decimal ProviderOpeningBalance { get => _ProviderOpeningBalance; set => _ProviderOpeningBalance = value; }
        private decimal _ProviderCloseingBalance;
        public decimal ProviderCloseingBalance { get => _ProviderCloseingBalance; set => _ProviderCloseingBalance = value; }

        private string _ProviderRequestID;
        public string ProviderRequestID { get => _ProviderRequestID; set => _ProviderRequestID = value; }

        private string _Status;
        public string Status { get => _Status.TrimIfNotNull(); set => _Status = value.TrimIfNotNull(); }
        private string _LapuNo;
        public string LapuNo { get => _LapuNo.TrimIfNotNull(); set => _LapuNo = value.TrimIfNotNull(); }

        private string _Response;
        public string Response { get => _Response; set => _Response = value; }

        private int _ProviderID;
        public int ProviderID { get => _ProviderID; set => _ProviderID = value; }

        private int _OperatorID;
        public int OperatorID { get => _OperatorID; set => _OperatorID = value; }

        private int _ServiceID;
        public int ServiceID { get => _ServiceID; set => _ServiceID = value; }

        private int _UserID;
        public int UserID { get => _UserID; set => _UserID = value; }
        public int TransactionMasterID { get; internal set; }
    }
}