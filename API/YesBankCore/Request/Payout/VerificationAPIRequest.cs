using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YesBankCore.Request.Payout
{
    public class VerificationAPIRequest
    {

        [JsonProperty("partnerReferenceNo")]
        public string partnerReferenceNo { get; set; }
        [JsonProperty("partnerKey")]
        public string partnerKey { get; set; }
        [JsonProperty("actionName")]
        public string actionName { get; set; }
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
       
    }
}
