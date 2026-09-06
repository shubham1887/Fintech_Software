using Newtonsoft.Json;

namespace Project.Requests.PaymentGateway
{
    public class InitTransactionRequest
    {
        [JsonProperty("body")]
        public Body_InitTransactionRequest Body { get; set; }

        [JsonProperty("head")]
        public Head_InitTransactionRequest Head { get; set; }

        public InitTransactionRequest()
        {
            Body = new Body_InitTransactionRequest();

            Head = new Head_InitTransactionRequest();
        }
    }

    public class Head_InitTransactionRequest
    {
        [JsonProperty("signature")]
        public string Signature { get; set; }
    }

    public class Body_InitTransactionRequest
    {
        [JsonProperty("requestType")]
        public string RequestType { get; set; }

        [JsonProperty("mid")]
        public string Mid { get; set; }

        [JsonProperty("websiteName")]
        public string WebsiteName { get; set; }

        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("callbackUrl")]
        public string CallbackUrl { get; set; }

        [JsonProperty("txnAmount")]
        public TxnAmount TxnAmount { get; set; }

        [JsonProperty("userInfo")]
        public UserInfo UserInfo { get; set; }
    }

    public class TxnAmount
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }
    }

    public class UserInfo
    {
        [JsonProperty("custId")]
        public string CustId { get; set; }
    }
}