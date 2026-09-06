using Newtonsoft.Json;

namespace MplanCore.Response
{
    public class InfoApiDTHPlanResponse
    {
        public string status { get; set; }
        public string message { get; set; }
        public string cust { get; set; }
        public string @operator { get; set; }
        public Data1 data { get; set; }
    }

    public class Rs1
    {
        [JsonProperty("1 Month")]
        public string _1Month { get; set; }
    }

    public class DTHPlan1
    {
        public Rs1 amount { get; set; }
        public string description { get; set; }
        public string category { get; set; }
        public string validity { get; set; }
    }

    public class DTHAddOnPack1
    {
        public Rs1 amount { get; set; }
        public string description { get; set; }
        public string category { get; set; }
        public string validity { get; set; }
    }

    public class Data1
    {
        public List<DTHPlan1> categories { get; set; }
        public List<DTHAddOnPack1> plans { get; set; }
    }
}