using Project.Processing;

namespace Project.Response.TransactionReport
{
    public class ManualSuccessFailViewModel
    {
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

        private string _ProviderName;

        public string ProviderName
        {
            get => _ProviderName.TrimIfNotNull();
            set => _ProviderName = value.TrimIfNotNull();
        }

        private string _OperatorName;

        public string OperatorName
        {
            get => _OperatorName.TrimIfNotNull();
            set => _OperatorName = value.TrimIfNotNull();
        }

        private string _ConsumerNumber;

        public string ConsumerNumber
        {
            get => _ConsumerNumber.TrimIfNotNull();
            set => _ConsumerNumber = value.TrimIfNotNull();
        }

        public decimal Amount { set; get; }

        private string _OldStatus;

        public string OldStatus
        {
            get => _OldStatus.TrimIfNotNull();
            set => _OldStatus = value.TrimIfNotNull();
        }

        private string _NewStatus;

        public string NewStatus
        {
            get => _NewStatus.TrimIfNotNull();
            set => _NewStatus = value.TrimIfNotNull();
        }

        private string _UpdatedBy;

        public string UpdatedBy
        {
            get => _UpdatedBy.TrimIfNotNull();
            set => _UpdatedBy = value.TrimIfNotNull();
        }

        private string _UpdatedDOC;

        public string UpdatedDOC
        {
            get => _UpdatedDOC.TrimIfNotNull();
            set => _UpdatedDOC = value.TrimIfNotNull();
        }

        private double _ProcTime;

        public double ProcTime
        {
            get => _ProcTime;
            set => _ProcTime = value;
        }

        public int TransactionMasterID { set; get; }

        internal int UserID { set; get; }

        private string _ReferenceNo;

        public string ReferenceNo
        {
            get => _ReferenceNo.TrimIfNotNull();
            set => _ReferenceNo = value.TrimIfNotNull();
        }

        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }
    }
}