using YESBankCore.Controller;

namespace YesBankCore.Request.BBPS
{
    public class StatusCheckAPIRequest
    {

        private string _billerId;
        public string billerId
        {
            get => _billerId.TrimIfNotNull();
            set => _billerId = value.TrimIfNotNull();
        }

        private string _txnReferenceId;
        public string txnReferenceId
        {
            get => _txnReferenceId.TrimIfNotNull();
            set => _txnReferenceId = value.TrimIfNotNull();
        }

        private string _npciTxnReferenceId;
        public string npciTxnReferenceId
        {
            get => _npciTxnReferenceId.TrimIfNotNull();
            set => _npciTxnReferenceId = value.TrimIfNotNull();
        }

        private string _xchangeId;
        public string xchangeId
        {
            get => _xchangeId.TrimIfNotNull();
            set => _xchangeId = value.TrimIfNotNull();
        }
    }
}
