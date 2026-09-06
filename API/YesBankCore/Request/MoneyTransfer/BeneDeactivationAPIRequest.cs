using YESBankCore.Controller;

namespace YesBankCore.Request.MoneyTransfer
{
    internal class BeneDeactivationAPIRequest
    {
        private string _txnId;

        public string txnId
        {
            get => _txnId.TrimIfNotNull();
            set => _txnId = value.TrimIfNotNull();
        }

        private string _customerMob;

        public string customerMob
        {
            get => _customerMob.TrimIfNotNull();
            set => _customerMob = value.TrimIfNotNull();
        }

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

        private string _hashKey;

        public string hashKey
        {
            get => _hashKey.TrimIfNotNull();
            set => _hashKey = value.TrimIfNotNull();
        }

        private string _channel;

        public string channel
        {
            get => _channel.TrimIfNotNull();
            set => _channel = value.TrimIfNotNull();
        }

        private string _benefAccNo;

        public string benefAccNo
        {
            get => _benefAccNo.TrimIfNotNull();
            set => _benefAccNo = value.TrimIfNotNull();
        }

        private string _benefIfscCode;

        public string benefIfscCode
        {
            get => _benefIfscCode.TrimIfNotNull();
            set => _benefIfscCode = value.TrimIfNotNull();
        }

        //public List<string> productCode { get; set; }

        private string _productCode;

        public string productCode
        {
            get => _productCode.TrimIfNotNull();
            set => _productCode = value.TrimIfNotNull();
        }

        private string _status;

        public string status
        {
            get => _status.TrimIfNotNull();
            set => _status = value.TrimIfNotNull();
        }
    }
}
