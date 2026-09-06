using Newtonsoft.Json;

namespace Project.Response.PaymentGateway
{
    public class InitTransactionResponse
    {
        [JsonProperty("head")]
        public Head Head { get; set; }

        [JsonProperty("body")]
        public Body Body { get; set; }
    }

    public class Body
    {
        [JsonProperty("resultInfo")]
        public ResultInfo ResultInfo { get; set; }

        [JsonProperty("txnToken")]
        public string TxnToken { get; set; }

        [JsonProperty("isPromoCodeValid")]
        public bool IsPromoCodeValid { get; set; }

        [JsonProperty("authenticated")]
        public bool Authenticated { get; set; }
    }

    public class ResultInfo
    {
        [JsonProperty("resultStatus")]
        public string ResultStatus { get; set; }

        [JsonProperty("resultCode")]
        public string ResultCode { get; set; }

        [JsonProperty("resultMsg")]
        public string ResultMsg { get; set; }
    }

    public class Head
    {
        [JsonProperty("responseTimestamp")]
        public string ResponseTimestamp { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("clientId")]
        public string ClientId { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }
    }
}