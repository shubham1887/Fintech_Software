using Newtonsoft.Json;
using YESBankCore.Controller;

namespace YESBankCore.Response
{
    public class QRPayResponse
    {
        private string _status;

        [JsonProperty("status")]
        public string Status
        {
            get => _status.TrimIfNotNull();
            set => _status = value.TrimIfNotNull();
        }

        private string _Response;

        [JsonProperty("actCode")]
        public string ActCode
        {
            get => _Response.TrimIfNotNull();
            set => _Response = value.TrimIfNotNull();
        }

        private string _MerchantId;

        [JsonProperty("merchantId")]
        public string MerchantId
        {
            get => _MerchantId.TrimIfNotNull();
            set => _MerchantId = value.TrimIfNotNull();
        }

        private string _TerminalId;

        [JsonProperty("terminalId")]
        public string TerminalId
        {
            get => _TerminalId.TrimIfNotNull();
            set => _TerminalId = value.TrimIfNotNull();
        }

        private string _Success;

        [JsonProperty("success")]
        public string Success
        {
            get => _Success.TrimIfNotNull();
            set => _Success = value.TrimIfNotNull();
        }

        private string _Message;

        [JsonProperty("message")]
        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private string _MerchantTranId;

        [JsonProperty("merchantTranId")]
        public string MerchantTranId
        {
            get => _MerchantTranId.TrimIfNotNull();
            set => _MerchantTranId = value.TrimIfNotNull();
        }

        private string _BankRefId;

        [JsonProperty("refId")]
        public string BankRefId
        {
            get => _BankRefId.TrimIfNotNull();
            set => _BankRefId = value.TrimIfNotNull();
        }
    }
}