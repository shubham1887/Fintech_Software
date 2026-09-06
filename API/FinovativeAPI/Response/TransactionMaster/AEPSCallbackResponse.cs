using Newtonsoft.Json;

namespace Project.Response.TransactionMaster
{
    public class AEPSCallbackResponse
    {
        public AEPSCallbackResponse()
        {
            Data = new CallbackData();
        }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public CallbackData Data { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }
    }

    public class CallbackData
    {
        [JsonProperty("aeps_txn_id")]
        public string AepsTxnId { get; set; }

        [JsonProperty("txn_req_id")]
        public string TxnReqId { get; set; }

        [JsonProperty("partner_agent_id")]
        public string PartnerAgentId { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("status")]
        public long Status { get; set; }

        [JsonProperty("remark")]
        public string Remark { get; set; }

        [JsonProperty("bank_rrn")]
        public string BankRRN { get; set; }

        [JsonProperty("udd")]
        public string UDD { get; set; }
    }
}