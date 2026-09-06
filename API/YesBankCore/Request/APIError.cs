using Newtonsoft.Json;

namespace YESBankCore.Request
{
    internal class APIError
    {
        [JsonProperty("httpCode")]
        public string httpCode { get; set; }

        [JsonProperty("httpMessage")]
        public string httpMessage { get; set; }

        [JsonProperty("moreInformation")]
        public string moreInformation { get; set; }

    }
}
