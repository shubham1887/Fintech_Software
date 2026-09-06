using YESBankCore.Controller;

namespace YESBankCore.Response
{
    public class QRInitResponse
    {
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

        private string _MerchantName;

        public string MerchantName
        {
            get => _MerchantName.TrimIfNotNull();
            set => _MerchantName = value.TrimIfNotNull();
        }

        private string _AdminVPA;

        public string AdminVPA
        {
            get => _AdminVPA.TrimIfNotNull();
            set => _AdminVPA = value.TrimIfNotNull();
        }

        private string _MCCCode;

        public string MCCCode
        {
            get => _MCCCode.TrimIfNotNull();
            set => _MCCCode = value.TrimIfNotNull();
        }
    }
}