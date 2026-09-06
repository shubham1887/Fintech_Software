using YESBankCore.Controller;

namespace YesBankCore.Response.BBPSResponse
{
    public class AdditionalInfo
    {
        private string _name;
        public string name
        {
            get => _name.TrimIfNotNull();
            set => _name = value.TrimIfNotNull();
        }
        private string _value;
        public string value
        {
            get => _value.TrimIfNotNull();
            set => _value = value.TrimIfNotNull();
        }
    }
    public class BillTag
    {
        private string _name;
        public string name
        {
            get => _name.TrimIfNotNull();
            set => _name = value.TrimIfNotNull();
        }
        private string _value;
        public string value
        {
            get => _value.TrimIfNotNull();
            set => _value = value.TrimIfNotNull();
        }
        
    }
    public class BillerResponse
    {

        private string _customerName;
        public string customerName
        {
            get => _customerName.TrimIfNotNull();
            set => _customerName = value.TrimIfNotNull();
        }

        private string _amount;
        public string amount
        {
            get => _amount.TrimIfNotNull();
            set => _amount = value.TrimIfNotNull();
        }

        private string _dueDate;
        public string dueDate
        {
            get => _dueDate.TrimIfNotNull();
            set => _dueDate = value.TrimIfNotNull();
        }
        
        private string _custConvFee;
        public string custConvFee
        {
            get => _custConvFee.TrimIfNotNull();
            set => _custConvFee = value.TrimIfNotNull();
        }
        
        private string _custConvDesc;
        public string custConvDesc
        {
            get => _custConvDesc.TrimIfNotNull();
            set => _custConvDesc = value.TrimIfNotNull();
        }
        
        private string _billDate;
        public string billDate
        {
            get => _billDate.TrimIfNotNull();
            set => _billDate = value.TrimIfNotNull();
        }
        
        private string _billNumber;
        public string billNumber
        {
            get => _billNumber.TrimIfNotNull();
            set => _billNumber = value.TrimIfNotNull();
        }
        
        private string _billPeriod;
        public string billPeriod
        {
            get => _billPeriod.TrimIfNotNull();
            set => _billPeriod = value.TrimIfNotNull();
        }

        public List<BillTag> billTags { get; set; }
    }
    public class BillDetail
    {
        private string _name;
        public string name
        {
            get => _name.TrimIfNotNull();
            set => _name = value.TrimIfNotNull();
        }
        private string _value;
        public string value
        {
            get => _value.TrimIfNotNull();
            set => _value = value.TrimIfNotNull();
        }
       
    }
    public class Response
    {
        private int _chId;
        public int chId
        {
            get => _chId;
            set => _chId = value;
        }
        
        private string _refId;
        public string refId
        {
            get => _refId.TrimIfNotNull();
            set => _refId = value.TrimIfNotNull();
        }
        
        private string _approvalRefNum;
        public string approvalRefNum
        {
            get => _approvalRefNum.TrimIfNotNull();
            set => _approvalRefNum = value.TrimIfNotNull();
        }
        
        private string _responseCode;
        public string responseCode
        {
            get => _responseCode.TrimIfNotNull();
            set => _responseCode = value.TrimIfNotNull();
        }
        
        private string _responseReason;
        public string responseReason
        {
            get => _responseReason.TrimIfNotNull();
            set => _responseReason = value.TrimIfNotNull();
        }
        
        private string _complianceReason;
        public string complianceReason
        {
            get => _complianceReason.TrimIfNotNull();
            set => _complianceReason = value.TrimIfNotNull();
        }
        
        private string _complianceRespCd;
        public string complianceRespCd
        {
            get => _complianceRespCd.TrimIfNotNull();
            set => _complianceRespCd = value.TrimIfNotNull();
        }
       
        public List<BillDetail> billDetails { get; set; }
        public BillerResponse billerResponse { get; set; }
        public List<AdditionalInfo> additionalInfo { get; set; }
        public DateTime txnDateTime { get; set; }
    }
    public class BillFetchAPIResponse
    {

        private string _respCode;
        public string respCode
        {
            get => _respCode.TrimIfNotNull();
            set => _respCode = value.TrimIfNotNull();
        }
        private string _status;
        public string status
        {
            get => _status.TrimIfNotNull();
            set => _status = value.TrimIfNotNull();
        }
   
        public Response response { get; set; }
    }
}
