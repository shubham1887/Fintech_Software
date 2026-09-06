using Newtonsoft.Json;

namespace Project.Response.ROffer
{
    internal class PlanResponse
    {
        [JsonProperty("records")]
        public Records Records { get; set; }

        [JsonProperty("status")]
        public long Status { get; set; }
    }

    public class Records
    {
        [JsonProperty("FULLTT")]
        public The2G[] FULLTT { get; set; }

        [JsonProperty("TOPUP")]
        public The2G[] TOPUP { get; set; }

        [JsonProperty("3G/4G")]
        public The2G[] The3G4G { get; set; }

        [JsonProperty("RATE CUTTER")]
        public The2G[] RateCutter { get; set; }

        [JsonProperty("2G")]
        public The2G[] The2G { get; set; }

        [JsonProperty("SMS")]
        public The2G[] SMS { get; set; }

        [JsonProperty("Romaing")]
        public The2G[] Romaing { get; set; }

        [JsonProperty("COMBO")]
        public The2G[] COMBO { get; set; }

        [JsonProperty("FRC")]
        public The2G[] FRC { get; set; }

        private string _message;

        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }
    }

    public class The2G
    {
        [JsonProperty("rs")]
        public string Rs { get; set; }

        [JsonProperty("desc")]
        public string Desc { get; set; }

        [JsonProperty("validity")]
        public string Validity { get; set; }

        [JsonProperty("last_update")]
        public string LastUpdate { get; set; }
    }
}