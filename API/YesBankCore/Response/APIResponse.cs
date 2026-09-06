using Newtonsoft.Json;

namespace YesBankCore.Response
{
    public class APIResponse
    {
      
        [JsonProperty("body")]
        public string body { get; set; }
        [JsonProperty("header")]
        public Header header { get; set; }
        public APIResponse()
        {
            header = new Header();
        }


        [JsonProperty("status")]
        public string status { get; set; }
        [JsonProperty("responseCode")]
        public string responseCode { get; set; }

        [JsonProperty("responseMessage")]
        public string responseMessage { get; set; }
      
     
     

    }
    public class Header
    {
        public string hash { get; set; }
        public string iv { get; set; }
        public string key { get; set; }

    }
}

