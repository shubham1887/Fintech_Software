using Newtonsoft.Json;

namespace YesBankCore.Request.Payout
{
    public class PayoutStatusCheckAPIRequest
    {
        [JsonProperty("requestId")]
        public string requestId { get; set; }
        [JsonProperty("actionName")]
        public string actionName { get; set; }
        [JsonProperty("partnerKey")]
        public string partnerKey { get; set; }
        [JsonProperty("p1")]
        public string p1 { get; set; }
    }
}
