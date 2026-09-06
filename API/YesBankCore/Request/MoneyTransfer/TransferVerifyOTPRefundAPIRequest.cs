using YESBankCore.Controller;

namespace YesBankCore.Request.MoneyTransfer
{
    internal class TransferVerifyOTPRefundAPIRequest
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

        private string _orderId;

        public string orderId
        {
            get => _orderId.TrimIfNotNull();
            set => _orderId = value.TrimIfNotNull();
        }

        private string _channel;

        public string channel
        {
            get => _channel.TrimIfNotNull();
            set => _channel = value.TrimIfNotNull();
        }

        private string _sortxnId;

        public string sortxnId
        {
            get => _sortxnId.TrimIfNotNull();
            set => _sortxnId = value.TrimIfNotNull();
        }

        private string _narration;

        public string narration
        {
            get => _narration.TrimIfNotNull();
            set => _narration = value.TrimIfNotNull();
        }

        private string _hashKey;

        public string hashKey
        {
            get => _hashKey.TrimIfNotNull();
            set => _hashKey = value.TrimIfNotNull();
        }

        private string _otp;

        public string otp
        {
            get => _otp.TrimIfNotNull();
            set => _otp = value.TrimIfNotNull();
        }

        private string _partnerStateCode;

        public string partnerStateCode
        {
            get => _partnerStateCode.TrimIfNotNull();
            set => _partnerStateCode = value.TrimIfNotNull();
        }

        private string _partnerDistrictCode;

        public string partnerDistrictCode
        {
            get => _partnerDistrictCode.TrimIfNotNull();
            set => _partnerDistrictCode = value.TrimIfNotNull();
        }

        private string _agentStateCode;

        public string agentStateCode
        {
            get => _agentStateCode.TrimIfNotNull();
            set => _agentStateCode = value.TrimIfNotNull();
        }

        private string _agentDistrictCode;

        public string agentDistrictCode
        {
            get => _agentDistrictCode.TrimIfNotNull();
            set => _agentDistrictCode = value.TrimIfNotNull();
        }

        private string _customerStateCode;

        public string customerStateCode
        {
            get => _customerStateCode.TrimIfNotNull();
            set => _customerStateCode = value.TrimIfNotNull();
        }

        private string _customerDistrictCode;

        public string customerDistrictCode
        {
            get => _customerDistrictCode.TrimIfNotNull();
            set => _customerDistrictCode = value.TrimIfNotNull();
        }
    }
}
