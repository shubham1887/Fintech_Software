using YESBankCore.Controller;

namespace YesBankCore.Request.MoneyTransfer
{
    internal class TransferAPIRequest
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

        //public List<string> productCode { get; set; }
        public string productCode { get; set; }

        private string _purposeCode;

        public string purposeCode
        {
            get => _purposeCode.TrimIfNotNull();
            set => _purposeCode = value.TrimIfNotNull();
        }

        private string _paymentMode;

        public string paymentMode
        {
            get => _paymentMode.TrimIfNotNull();
            set => _paymentMode = value.TrimIfNotNull();
        }

        private string _amount;

        public string amount
        {
            get => _amount.TrimIfNotNull();
            set => _amount = value.TrimIfNotNull();
        }

        private string _currencyCode;

        public string currencyCode
        {
            get => _currencyCode.TrimIfNotNull();
            set => _currencyCode = value.TrimIfNotNull();
        }

        private string _beneAccountNo;

        public string beneAccountNo
        {
            get => _beneAccountNo.TrimIfNotNull();
            set => _beneAccountNo = value.TrimIfNotNull();
        }

        private string _beneIfsc;

        public string beneIfsc
        {
            get => _beneIfsc.TrimIfNotNull();
            set => _beneIfsc = value.TrimIfNotNull();
        }

        private string _beneMobileNo;

        public string beneMobileNo
        {
            get => _beneMobileNo.TrimIfNotNull();
            set => _beneMobileNo = value.TrimIfNotNull();
        }

        private string _beneEmailId;

        public string beneEmailId
        {
            get => _beneEmailId.TrimIfNotNull();
            set => _beneEmailId = value.TrimIfNotNull();
        }

        private string _beneName;

        public string beneName
        {
            get => _beneName.TrimIfNotNull();
            set => _beneName = value.TrimIfNotNull();
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
    }
}
