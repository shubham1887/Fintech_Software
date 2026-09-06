using Newtonsoft.Json;
using YESBankCore.Controller;

namespace YesBankCore.Response
{
    public class BalanceResponse
    {
        private string _Status;

        [JsonProperty("status")]
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

        private string _Balance;

        [JsonProperty("Balance")]
        public string Balance
        {
            get => _Balance.TrimIfNotNull();
            set => _Balance = value.TrimIfNotNull();
        }
    }
}