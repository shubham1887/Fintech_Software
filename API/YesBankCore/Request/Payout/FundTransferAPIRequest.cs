using Newtonsoft.Json;

namespace YesBankCore.Request.Payout
{
    public class FundTransferAPIRequest
    {
        
        [JsonProperty("partnerReferenceNo")]
        public string partnerReferenceNo { get; set; }
        [JsonProperty("actionName")]
        public string actionName { get; set; }
        [JsonProperty("partnerKey")]
        public string partnerKey { get; set; }

        [JsonProperty("p1")]
        public string p1 { get; set; }
        [JsonProperty("p2")]
        public string p2 { get; set; }
        [JsonProperty("p3")]
        public string p3 { get; set; }
        [JsonProperty("p4")]
        public string p4 { get; set; }
        [JsonProperty("p5")]
        public string p5 { get; set; }
        [JsonProperty("p6")]
        public string p6 { get; set; }
        [JsonProperty("p7")]
        public string p7 { get; set; }
        [JsonProperty("p8")]
        public string p8 { get; set; }
      //  [JsonProperty("txnId")]
     //   public string txnId { get; set; }


    }
}
