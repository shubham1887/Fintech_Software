using YESBankCore.Controller;

namespace YesBankCore.Request.MoneyTransfer
{
    internal class StatusCheckAPIRequest
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

        private string _orgTxnId;

        public string orgTxnId
        {
            get => _orgTxnId.TrimIfNotNull();
            set => _orgTxnId = value.TrimIfNotNull();
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

        private string _fromDate;

        public string fromDate
        {
            get => _fromDate.TrimIfNotNull();
            set => _fromDate = value.TrimIfNotNull();
        }

        private string _toDate;

        public string toDate
        {
            get => _toDate.TrimIfNotNull();
            set => _toDate = value.TrimIfNotNull();
        }

        private string _hashKey;

        public string hashKey
        {
            get => _hashKey.TrimIfNotNull();
            set => _hashKey = value.TrimIfNotNull();
        }

    }
}
