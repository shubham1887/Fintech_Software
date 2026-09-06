using Newtonsoft.Json;

namespace Project.Response.PaymentGateway
{
    public class PaytmStatusCheckResponse
    {
        [JsonProperty("TXNID")]
        public string Txnid { get; set; }

        [JsonProperty("BANKTXNID")]
        public string Banktxnid { get; set; }

        [JsonProperty("ORDERID")]
        public string Orderid { get; set; }

        [JsonProperty("TXNAMOUNT")]
        public string Txnamount { get; set; }

        [JsonProperty("STATUS")]
        public string Status { get; set; }

        [JsonProperty("TXNTYPE")]
        public string Txntype { get; set; }

        [JsonProperty("GATEWAYNAME")]
        public string Gatewayname { get; set; }

        [JsonProperty("RESPCODE")]
        public string Respcode { get; set; }

        [JsonProperty("RESPMSG")]
        public string Respmsg { get; set; }

        [JsonProperty("BANKNAME")]
        public string Bankname { get; set; }

        [JsonProperty("MID")]
        public string Mid { get; set; }

        [JsonProperty("PAYMENTMODE")]
        public string Paymentmode { get; set; }

        [JsonProperty("REFUNDAMT")]
        public string Refundamt { get; set; }

        [JsonProperty("TXNDATE")]
        public string Txndate { get; set; }
    }
}