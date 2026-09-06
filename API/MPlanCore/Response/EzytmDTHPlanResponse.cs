using Newtonsoft.Json;

namespace MplanCore.Response
{
    public class EzytmDTHPlanResponse
    {
        public string ERROR { get; set; }
        public string STATUS { get; set; }
        public string Operator { get; set; }
        public string Circle { get; set; }
        public RDATADTH RDATA { get; set; }
        public string MESSAGE { get; set; }
    }

    public class Rs
    {
        [JsonProperty("1 MONTHS")]
        public string _1MONTHS { get; set; }
    }

    public class DTHPlan
    {
        public Rs rs { get; set; }
        public string desc { get; set; }
        public string plan_name { get; set; }
        public string last_update { get; set; }
    }

    public class DTHAddOnPack
    {
        public Rs rs { get; set; }
        public string desc { get; set; }
        public string plan_name { get; set; }
        public string last_update { get; set; }
    }

    public class RDATADTH
    {
        public List<DTHPlan> Plan { get; set; }

        [JsonProperty("Add-OnPack")]
        public List<DTHAddOnPack> AddOnPack { get; set; }
    }
}