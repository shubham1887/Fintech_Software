using Project.Processing;

namespace Project.Response.ComplaintResponse
{
    public class ViewModel
    {
        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private string _ComplaintType;
        public string ComplaintType { get => _ComplaintType; set => _ComplaintType = value; }

        private string _Operator;

        public string Operator
        {
            get => _Operator.TrimIfNotNull();
            set => _Operator = value.TrimIfNotNull();
        }

        private string _ConsumerNumber;

        public string ConsumerNumber
        {
            get => _ConsumerNumber.TrimIfNotNull();
            set => _ConsumerNumber = value.TrimIfNotNull();
        }

        private string _ProviderRefNo;

        public string ProviderRefNo
        {
            get => _ProviderRefNo.TrimIfNotNull();
            set => _ProviderRefNo = value.TrimIfNotNull();
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private string _ComplaintStatus;

        public string ComplaintStatus
        {
            get => _ComplaintStatus.TrimIfNotNull();
            set => _ComplaintStatus = value.TrimIfNotNull();
        }

        private string _CustomerRemark;

        public string CustomerRemark
        {
            get => _CustomerRemark.TrimIfNotNull();
            set => _CustomerRemark = value.TrimIfNotNull();
        }

        private string _AdminRemark;

        public string AdminRemark
        {
            get => _AdminRemark.TrimIfNotNull();
            set => _AdminRemark = value.TrimIfNotNull();
        }

        private string _DOC;

        public string DOC
        {
            get => _DOC.TrimIfNotNull();
            set => _DOC = value.TrimIfNotNull();
        }

        private string _TransactionDate;

        public string TransactionDate
        {
            get => _TransactionDate.TrimIfNotNull();
            set => _TransactionDate = value.TrimIfNotNull();
        }

        private string _ProviderName;

        public string ProviderName
        {
            get => _ProviderName.TrimIfNotNull();
            set => _ProviderName = value.TrimIfNotNull();
        }

        private int _TransactionMasterID;

        public int TransactionMasterID
        {
            get => _TransactionMasterID;
            set => _TransactionMasterID = value;
        }

        private int _ComplaintMasterID;

        public int ComplaintMasterID
        {
            get => _ComplaintMasterID;
            set => _ComplaintMasterID = value;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private string _UpdatedDOC;

        public string UpdatedDOC
        {
            get => _UpdatedDOC.TrimIfNotNull();
            set => _UpdatedDOC = value.TrimIfNotNull();
        }

        private string _ReferenceNo;
        public string ReferenceNo
        {
            get => _ReferenceNo.ToUpperCase();
            set => _ReferenceNo = value.ToUpperCase();
        }

        private string _Service;

        public string Service
        {
            get => _Service.TrimIfNotNull();
            set => _Service = value.TrimIfNotNull();
        }
        private string _DateType;

        public string DateType
        {
            get => _DateType.TrimIfNotNull();
            set => _DateType = value.TrimIfNotNull();
        }

        public int OperatorID { get => _OperatorID; set => _OperatorID = value; }

        private int _OperatorID;

        private string _NewConsumerNumber;
        public string NewConsumerNumber { get => _NewConsumerNumber; set => _NewConsumerNumber = value; }

        private string _SupportNumber;

        public string SupportNumber
        {
            get => _SupportNumber.TrimIfNotNull();
            set => _SupportNumber = value.TrimIfNotNull();
        }

        private string _ProviderRequestID;

        public string ProviderRequestID
        {
            get => _ProviderRequestID.TrimIfNotNull();
            set => _ProviderRequestID = value.TrimIfNotNull();
        }

        private string _StringAmount;

        public string StringAmount
        {
            get => _StringAmount;
            set => _StringAmount = value;
        }

        private string _Logo;

        public string Logo
        {
            get => _Logo.TrimIfNotNull();
            set => _Logo = value.TrimIfNotNull();
        }

        public bool? IsBBPS { get; internal set; }

        private string _OldStatus;

        public string OldStatus
        {
            get => _OldStatus.TrimIfNotNull();
            set => _OldStatus = value.TrimIfNotNull();
        }
    }
}