using YESBankCore.Controller;

namespace YesBankCore.Request.MoneyTransfer
{
    internal class SenderSelectAPIRequest
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

        //private string _productCode;

        //public string productCode
        //{
        //    get => _productCode.TrimIfNotNull();
        //    set => _productCode = value.TrimIfNotNull();
        //}
        public List<string> productCode { get; set; }

        private string _channel;

        public string channel
        {
            get => _channel.TrimIfNotNull();
            set => _channel = value.TrimIfNotNull();
        }

        private string _isOTPValidated;

        public string isOTPValidated
        {
            get => _isOTPValidated.TrimIfNotNull();
            set => _isOTPValidated = value.TrimIfNotNull();
        }

        private string _hashKey;

        public string hashKey
        {
            get => _hashKey.TrimIfNotNull();
            set => _hashKey = value.TrimIfNotNull();
        }
    }
}
