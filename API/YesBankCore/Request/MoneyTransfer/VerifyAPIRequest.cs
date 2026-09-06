using YESBankCore.Controller;

namespace YesBankCore.Request.MoneyTransfer
{
    internal class VerifyAPIRequest
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

        private string _isUpi;

        public string isUpi
        {
            get => _isUpi.TrimIfNotNull();
            set => _isUpi = value.TrimIfNotNull();
        }

        private string _hashKey;

        public string hashKey
        {
            get => _hashKey.TrimIfNotNull();
            set => _hashKey = value.TrimIfNotNull();
        }
    }
}
