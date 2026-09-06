using Newtonsoft.Json;
using Project.Processing;

namespace Project.Response.CallbackMaster
{
    public class PhonePayCallback
    {
        public bool success { get; set; }

        private string _code;

        [JsonProperty("code")]
        public string code
        {
            get => _code.TrimIfNotNull();
            set => _code = value.TrimIfNotNull();
        }

        private string _message;

        [JsonProperty("message")]
        public string message
        {
            get => _message.TrimIfNotNull();
            set => _message = value.TrimIfNotNull();
        }

        public StatusData data { get; set; } = new StatusData();
    }

    public class StatusData
    {
        private string _merchantId;

        [JsonProperty("merchantId")]
        public string merchantId
        {
            get => _merchantId.TrimIfNotNull();
            set => _merchantId = value.TrimIfNotNull();
        }

        private string _merchantTransactionId;

        [JsonProperty("merchantTransactionId")]
        public string merchantTransactionId
        {
            get => _merchantTransactionId.TrimIfNotNull();
            set => _merchantTransactionId = value.TrimIfNotNull();
        }

        private string _transactionId;

        [JsonProperty("transactionId")]
        public string transactionId
        {
            get => _transactionId.TrimIfNotNull();
            set => _transactionId = value.TrimIfNotNull();
        }

        public int amount { get; set; }

        private string _state;

        [JsonProperty("state")]
        public string state
        {
            get => _state.TrimIfNotNull();
            set => _state = value.TrimIfNotNull();
        }

        private string _responseCode;

        [JsonProperty("responseCode")]
        public string responseCode
        {
            get => _responseCode.TrimIfNotNull();
            set => _responseCode = value.TrimIfNotNull();
        }

        private string _responseCodeDescription;

        [JsonProperty("responseCodeDescription")]
        public string responseCodeDescription
        {
            get => _responseCodeDescription.TrimIfNotNull();
            set => _responseCodeDescription = value.TrimIfNotNull();
        }

        public PaymentInstrument paymentInstrument { get; set; } = new PaymentInstrument();
    }

    public class PaymentInstrument
    {
        private string _type;

        [JsonProperty("type")]
        public string type
        {
            get => _type.TrimIfNotNull();
            set => _type = value.TrimIfNotNull();
        }

        private string _utr;

        [JsonProperty("utr")]
        public string utr
        {
            get => _utr.TrimIfNotNull();
            set => _utr = value.TrimIfNotNull();
        }
    }
}