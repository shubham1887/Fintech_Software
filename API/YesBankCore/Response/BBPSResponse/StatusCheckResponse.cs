using YESBankCore.Controller;

namespace YesBankCore.Response.BBPSResponse
{
    public class StatusCheckResponse
    {
        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }
        
        private string _TransactionID;

        public string TransactionID
        {
            get => _TransactionID.TrimIfNotNull();
            set => _TransactionID = value.TrimIfNotNull();
        }
        
        private string _TxnReferenceId;

        public string TxnReferenceId
        {
            get => _TxnReferenceId.TrimIfNotNull();
            set => _TxnReferenceId = value.TrimIfNotNull();
        }

    }
}
