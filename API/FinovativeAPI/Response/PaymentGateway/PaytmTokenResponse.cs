using Newtonsoft.Json;

namespace Project.Response.PaymentGateway
{
    public class PaytmTokenResponse
    {
        [JsonProperty("orderid")]
        public string OrderID { get; set; }

        [JsonProperty("mid")]
        public string MID { get; set; }

        [JsonProperty("txnToken")]
        public string TxnToken { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("callbackurl")]
        public string Callbackurl { get; set; }

        public string Message { get; internal set; }
    }
}