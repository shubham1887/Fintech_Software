using Newtonsoft.Json;

namespace Project.Response.BankIT
{
    public class BankITCallbackResponse
    {
        [JsonProperty("ClientRefID")]
        public string ClientRefID { get; set; }

        [JsonProperty("txnStatus")]
        public string txnStatus { get; set; }

        [JsonProperty("RRN")]
        public string RRN { get; set; }

        [JsonProperty("amount")]
        public string amount { get; set; }

        [JsonProperty("errorCode")]
        public string errorCode { get; set; }

        [JsonProperty("errorMsg")]
        public string errorMsg { get; set; }

        [JsonProperty("service")]
        public string service { get; set; }

        [JsonProperty("agentId")]
        public string agentId { get; set; }

        [JsonProperty("mobile")]
        public string mobile { get; set; }

        [JsonProperty("cardNo")]
        public string cardNo { get; set; }

        [JsonProperty("cardBalance")]
        public string cardBalance { get; set; }

        [JsonProperty("transactionDatetime")]
        public string transactionDatetime { get; set; }

        [JsonProperty("deviceNo")]
        public string deviceNo { get; set; }

        [JsonProperty("operator")]
        public string Operator { get; set; }
    }
}