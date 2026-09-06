using Newtonsoft.Json;

namespace YESBankCore.Request
{
    internal class APIRequest
    {
        [JsonProperty("requestMsg")]
        public string requestMsg { get; set; }

        [JsonProperty("pgMerchantId")]
        public string pgMerchantId { get; set; }
    }
}