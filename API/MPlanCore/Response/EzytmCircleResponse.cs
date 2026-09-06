using Newtonsoft.Json;

namespace MplanCore.Response
{
    internal class EzytmCircleResponse
    {
        [JsonProperty("ERROR")]
        public string Error { get; set; }

        [JsonProperty("STATUS")]
        public string Status { get; set; }

        [JsonProperty("Mobile")]
        public string Mobile { get; set; }

        [JsonProperty("Operator")]
        public string Operator { get; set; }

        [JsonProperty("Circle")]
        public string Circle { get; set; }

        [JsonProperty("CircleCode")]
        public string CircleCode { get; set; }

        [JsonProperty("Message")]
        public string Message { get; set; }
    }
}