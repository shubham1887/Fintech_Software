using Newtonsoft.Json;
using System.Collections.Generic;

namespace Project.Response.PaymentGateway
{
    public class RazorpayOrderResponse
    {
        public string id { get; set; }

        [JsonProperty("error")]
        public Error_RazorpayOrderResponse Error { get; set; }

        public string entity { get; set; }
        public int amount { get; set; }
        public int amount_paid { get; set; }
        public int amount_due { get; set; }
        public string currency { get; set; }
        public string receipt { get; set; }
        public string offer_id { get; set; }
        public string status { get; set; }
        public int attempts { get; set; }
        public List<string> notes { get; set; }
        public int created_at { get; set; }
    }

    public class Error_RazorpayOrderResponse
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("step")]
        public string Step { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }
    }
}