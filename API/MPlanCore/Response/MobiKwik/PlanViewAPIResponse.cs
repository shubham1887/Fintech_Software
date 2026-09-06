namespace MplanCore.Response.MobiKwik
{
    public class PlanViewAPIResponse
    {
        public bool success { get; set; }
        public Data5 data { get; set; }

        public Message1 message { get; set; }

    }

    public class Data5
    {
        public List<Plan> plans { get; set; }
    }

    public class Plan
    {
        public string id { get; set; }
        public int operatorId { get; set; }
        public int circleId { get; set; }
        public int planType { get; set; }
        public string planCode { get; set; }
        public string amount { get; set; }
        public double talktime { get; set; }
        public string validity { get; set; }
        public string planName { get; set; }
        public string planDescription { get; set; }
        public string dataBenefit { get; set; }
        public int isPopular { get; set; }
        public int isPromoted { get; set; }
        public int isHidden { get; set; }
        public object stringTalktime { get; set; }
        public bool special { get; set; }
        public int validityInDays { get; set; }
        public List<string> languageRegion { get; set; }
        public List<string> type { get; set; }
        public int planTypeForWeb { get; set; }
        public List<PlanDetailItemList> planDetailItemList { get; set; }
        public List<int> planTypeList { get; set; }
    }

    public class PlanDetailItemList
    {
        public string key { get; set; }
        public string displayName { get; set; }
        public string displayValue { get; set; }
    }

    public class Message1
    {
        public string code { get; set; }
        public string text { get; set; }
    }

}
