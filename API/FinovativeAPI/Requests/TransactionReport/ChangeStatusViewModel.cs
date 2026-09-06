using Project.Processing;

namespace Project.Requests.TransactionReport
{
    public class ChangeStatusViewModel
    {
        public int TransactionMasterID { set; get; }

        private string _DOC;

        public string DOC
        {
            get => _DOC.TrimIfNotNull();
            set => _DOC = value.TrimIfNotNull();
        }

        private string _ChangeStatusDOC;

        public string ChangeStatusDOC
        {
            get => _ChangeStatusDOC.TrimIfNotNull();
            set => _ChangeStatusDOC = value.TrimIfNotNull();
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

        private string _ChangeStatus;

        public string ChangeStatus
        {
            get => _ChangeStatus.TrimIfNotNull();
            set => _ChangeStatus = value.TrimIfNotNull();
        }

        public int OperatorID { set; get; }
        public int ServiceID { set; get; }

        private string _ProviderRequestID;

        public string ProviderRequestID
        {
            get => _ProviderRequestID.TrimIfNotNull();
            set => _ProviderRequestID = value.TrimIfNotNull();
        }

        private string _OperatorName;

        public string OperatorName
        {
            get => _OperatorName.TrimIfNotNull();
            set => _OperatorName = value.TrimIfNotNull();
        }

        private string _ProviderName;

        public string ProviderName
        {
            get => _ProviderName.TrimIfNotNull();
            set => _ProviderName = value.TrimIfNotNull();
        }

        private string _ReferenceNo;

        public string ReferenceNo
        {
            get => _ReferenceNo.ToUpperCase();
            set => _ReferenceNo = value.ToUpperCase();
        }

        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }
    }
}