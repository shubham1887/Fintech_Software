using YESBankCore.Controller;

namespace YesBankCore.Request.BBPS
{
    public class CustomerParam
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
    public class BillDetails
    {
        private string _billerId;
        public string billerId
        {
            get => _billerId.TrimIfNotNull();
            set => _billerId = value.TrimIfNotNull();
        }
        public List<CustomerParam> customerParams { get; set; }
    }
    public class DeviceTag
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
    public class AgentDetails
    {
        private string _agentId;
        public string agentId
        {
            get => _agentId.TrimIfNotNull();
            set => _agentId = value.TrimIfNotNull();
        }

        private string _partnerId;
        public string partnerId
        {
            get => _partnerId.TrimIfNotNull();
            set => _partnerId = value.TrimIfNotNull();
        }
        public List<DeviceTag> deviceTags { get; set; }
    }
    public class CustomerTag
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
    public class CustDetails
    {
        private string _mobileNo;
        public string mobileNo
        {
            get => _mobileNo.TrimIfNotNull();
            set => _mobileNo = value.TrimIfNotNull();
        }


        public List<CustomerTag> customerTags { get; set; }
    }
    public class BillFetchAPIRequest
    {
        private int _chId;
        public int chId
        {
            get => _chId;
            set => _chId = value;
        }
        public CustDetails custDetails { get; set; }
        public AgentDetails agentDetails { get; set; }
        public BillDetails billDetails { get; set; }
    }
}
