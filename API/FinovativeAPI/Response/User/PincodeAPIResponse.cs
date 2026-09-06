using MplanCore.Model;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Project.Response.User
{
    public class PincodeAPIResponse
    {
        public string Message { get; set; }
        public string Status { get; set; }

        [JsonProperty("PostOffice")]
        [JsonConverter(typeof(SingleOrArrayConverter<PostOffice>))]
        public List<PostOffice> PostOffice { get; set; }
    }

    public class PostOffice
    {
        public string Name { get; set; }
        public object Description { get; set; }
        public string BranchType { get; set; }
        public string DeliveryStatus { get; set; }
        public string Circle { get; set; }
        public string District { get; set; }
        public string Division { get; set; }
        public string Region { get; set; }
        public string Block { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Pincode { get; set; }
    }
}