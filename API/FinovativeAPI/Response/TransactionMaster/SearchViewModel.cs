using Project.Processing;

namespace Project.Response.TransactionMaster
{
    public class SearchViewModel
    {
        public int TransactionMasterID { set; get; }

        private string _DOC;

        public string DOC
        {
            get => _DOC.TrimIfNotNull();
            set => _DOC = value.TrimIfNotNull();
        }

        public int UserID { set; get; }

        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private string _ConsumerNumber;

        public string ConsumerNumber
        {
            get => _ConsumerNumber.TrimIfNotNull();
            set => _ConsumerNumber = value.TrimIfNotNull();
        }

        private string _SenderNo;

        public string SenderNo
        {
            get => _SenderNo.TrimIfNotNull();
            set => _SenderNo = value.TrimIfNotNull();
        }

        public decimal OpeningBalance { get; set; }

        public decimal Amount { set; get; }

        public decimal DebitAmount { set; get; }

        public decimal ClosingBalance { get; set; }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        public int OperatorID { set; get; }
        public int ServiceID { set; get; }

        private string _ProviderRequestID;

        public string ProviderRequestID
        {
            get => _ProviderRequestID.TrimIfNotNull();
            set => _ProviderRequestID = value.TrimIfNotNull();
        }

        public int ProviderID { get; internal set; }

        private string _OperatorName;

        public string OperatorName { get => _OperatorName.TrimIfNotNull(); set => _OperatorName = value.TrimIfNotNull(); }

        private string _ProviderName;
        public string ProviderName { get => _ProviderName.TrimIfNotNull(); set => _ProviderName = value.TrimIfNotNull(); }
        private string _ReferenceNo;

        public string ReferenceNo
        {
            get => _ReferenceNo.ToUpperCase();
            set => _ReferenceNo = value.ToUpperCase();
        }

        private int _DistributerUserID;

        public int DistributerUserID
        {
            get => _DistributerUserID;
            set => _DistributerUserID = value;
        }

        private int _SuperDistributerUserID;

        public int SuperDistributerUserID
        {
            get => _SuperDistributerUserID;
            set => _SuperDistributerUserID = value;
        }

        private string _ServiceName;

        public string ServiceName
        {
            get => _ServiceName.TrimIfNotNull();
            set => _ServiceName = value.TrimIfNotNull();
        }
    }
}