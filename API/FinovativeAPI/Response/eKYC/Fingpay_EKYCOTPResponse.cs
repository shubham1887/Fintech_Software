using Newtonsoft.Json;
using Project.Processing;

namespace Project.Response.eKYC
{
    public class Fingpay_EKYCOTPResponse
    {
        private string _Status;

        [JsonProperty("Status")]
        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _Message;

        [JsonProperty("Message")]
        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private string _EncodeFPTxnId;

        [JsonProperty("EncodeFPTxnId")]
        public string EncodeFPTxnId
        {
            get => _EncodeFPTxnId.TrimIfNotNull();
            set => _EncodeFPTxnId = value.TrimIfNotNull();
        }

        private int _PrimaryKeyId;

        [JsonProperty("PrimaryKeyId")]
        public int PrimaryKeyId
        {
            get => _PrimaryKeyId;
            set => _PrimaryKeyId = value;
        }
    }
}