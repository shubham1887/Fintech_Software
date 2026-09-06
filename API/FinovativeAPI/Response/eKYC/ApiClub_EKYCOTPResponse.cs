using Newtonsoft.Json;
using Project.Processing;

namespace Project.Response.eKYC
{
    public class ApiClub_EKYCOTPResponse
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

        private string _TransactionID;

        [JsonProperty("TransactionID")]
        public string TransactionID
        {
            get => _TransactionID.TrimIfNotNull();
            set => _TransactionID = value.TrimIfNotNull();
        }
    }
}