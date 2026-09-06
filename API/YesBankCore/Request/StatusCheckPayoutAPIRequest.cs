using Newtonsoft.Json;

namespace YesBankCore.Request
{
    internal class StatusCheckPayoutAPIRequest
    {
        [JsonProperty("Data")]
        public DataStatusCheck Data { get; set; }
    }

    internal class DataStatusCheck
    {
        [JsonProperty("InstrId")]
        public string OrderID { get; set; }

        [JsonProperty("ConsentId")]
        public string ConsentId { get; set; }

        [JsonProperty("SecondaryIdentification")]
        public string SecondaryIdentification { get; set; }
    }
}