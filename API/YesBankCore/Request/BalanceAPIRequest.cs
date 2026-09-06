using Newtonsoft.Json;

namespace YesBankCore.Request
{
    internal class BalanceAPIRequest
    {
        [JsonProperty("Data")]
        public BalanceDataRequest Data { get; set; }

        public BalanceAPIRequest()
        {
            Data = new BalanceDataRequest();
        }
    }

    internal class BalanceDataRequest
    {
        [JsonProperty("DebtorAccount")]
        public DebtorAccountBalanceRequest DebtorAccount { get; set; }

        public BalanceDataRequest()
        {
            DebtorAccount = new DebtorAccountBalanceRequest();
        }
    }

    internal class DebtorAccountBalanceRequest
    {
        [JsonProperty("ConsentId")]
        public string ConsentId { get; set; }

        [JsonProperty("Identification")]
        public string Identification { get; set; }

        [JsonProperty("SecondaryIdentification")]
        public string SecondaryIdentification { get; set; }
    }
}