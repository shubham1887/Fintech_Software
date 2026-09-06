using Newtonsoft.Json;
using YESBankCore.Controller;

namespace YesBankCore.Response.Payout
{
    public class BalanceFetchResponse
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
        private string _balance;

        [JsonProperty("balance")]
        public string balance
        {
            get => _balance.TrimIfNotNull();
            set => _balance = value.TrimIfNotNull();
        }

        private string _RequestId;

        [JsonProperty("RequestId")]
        public string RequestId
        {
            get => _RequestId.TrimIfNotNull();
            set => _RequestId = value.TrimIfNotNull();
        }
    }
}
