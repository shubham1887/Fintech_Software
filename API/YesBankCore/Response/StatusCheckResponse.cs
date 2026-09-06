using YESBankCore.Controller;

namespace YESBankCore.Response
{
    public class StatusCheckResponse
    {
        private string _OrderNo;

        public string OrderNo
        {
            get => _OrderNo.TrimIfNotNull();
            set => _OrderNo = value.TrimIfNotNull();
        }

        private string _status;

        public string Status
        {
            get => _status.TrimIfNotNull();
            set => _status = value.TrimIfNotNull();
        }

        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private string _Amount;

        public string Amount
        {
            get => _Amount.TrimIfNotNull();
            set => _Amount = value.TrimIfNotNull();
        }

        private string _StatusCode;

        public string StatusCode
        {
            get => _StatusCode.TrimIfNotNull();
            set => _StatusCode = value.TrimIfNotNull();
        }

        private string _TransactionID;

        public string TransactionID
        {
            get => _TransactionID.TrimIfNotNull();
            set => _TransactionID = value.TrimIfNotNull();
        }

        private string _CustomerVPA;

        public string CustomerVPA
        {
            get => _CustomerVPA.TrimIfNotNull();
            set => _CustomerVPA = value.TrimIfNotNull();
        }
    }
}