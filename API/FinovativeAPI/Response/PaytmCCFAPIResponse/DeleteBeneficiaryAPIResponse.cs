using Newtonsoft.Json;
using Project.Processing;

namespace Project.Response.PaytmCCFAPIResponse
{
    public class DeleteBeneficiaryAPIResponse
    {
        public string status { get; set; }

        public string message { get; set; }

        [JsonProperty("response_code")]
        public string ResponseCode { get; set; }

    }
}