using Newtonsoft.Json;

namespace YesBankCore.Request
{
    internal class StatusCheckAPIRequest
    {
        [JsonProperty("Data")]
        public StatusCheckRequest Data { get; set; }

        public StatusCheckAPIRequest()
        {
            Data = new StatusCheckRequest();
        }
    }

    internal class StatusCheckRequest
    {
        [JsonProperty("InstrId")]
        public string InstrId { get; set; }

        [JsonProperty("ConsentId")]
        public string ConsentId { get; set; }

        [JsonProperty("SecondaryIdentification")]
        public string SecondaryIdentification { get; set; }
    }
}