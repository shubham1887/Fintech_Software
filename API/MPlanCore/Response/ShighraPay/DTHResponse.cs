using Newtonsoft.Json;

namespace MplanCore.Response.ShighraPay
{
    public class DTHResponse
    {
        public int status_id { get; set; }
        public string message { get; set; }
        public string opid { get; set; }
        public PlansForDTH plans { get; set; }
    }

    public class PlansForDTH
    {
        public List<DTHPlans> Plan { get; set; }
    }

    public class DTHPlans
    {
        public Rs rs { get; set; }
        public string desc { get; set; }
        public string plan_name { get; set; }
        public string last_update { get; set; }
    }

    public class Rs
    {
        [JsonProperty("6 MONTHS")]
        public string _6MONTHS { get; set; }

        [JsonProperty("3 MONTHS")]
        public string _3MONTHS { get; set; }

        [JsonProperty("1 MONTHS")]
        public string _1MONTHS { get; set; }

        [JsonProperty("1 YEAR")]
        public string _1YEAR { get; set; }

        //private string _1YEAR;

        //[JsonProperty("1 YEAR")]
        //public string _1YEAR
        //{
        //    get => _1YEAR.TrimIfNotNull();
        //    set => _1YEAR = value.TrimIfNotNull();
        //}
    }
}