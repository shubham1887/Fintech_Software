using Newtonsoft.Json;

namespace YesBankCore.Request.Payout
{
    public class FetchBalanceAPIRequest
    {
        [JsonProperty("requestNumber")]
        public string requestNumber { get; set; }
        [JsonProperty("actionName")]
        public string actionName { get; set; }
        [JsonProperty("partnerKey")]
        public string partnerKey { get; set; }
        
    }
}
