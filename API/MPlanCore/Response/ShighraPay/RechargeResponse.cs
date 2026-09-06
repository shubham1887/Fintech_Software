using Newtonsoft.Json;

namespace MplanCore.Response.ShighraPay
{
    public class RechargeResponse
    {
        public int status_id { get; set; }
        public string message { get; set; }
        public string opid { get; set; }
        public Plans plans { get; set; }
    }

    public class Plans
    {
        public List<FULLTT> FULLTT { get; set; }
        public List<TOPUP> TOPUP { get; set; }

        [JsonProperty("3G/4G")]
        public List<_3G4G> _3G4G { get; set; }

        [JsonProperty("2G")]
        public List<_2G> _2G { get; set; }
        public List<Roaming> Roaming { get; set; }
    }

    public class _2G
    {
      public string rs { get; set; }
      public string desc { get; set; }
      public string validity { get; set; }
      public string last_update { get; set; }

    }

    public class _3G4G
    {
      public string rs { get; set; }
      public string desc { get; set; }
      public string validity { get; set; }
      public string last_update { get; set; }
    } 

    public class Roaming
    {
        public string rs { get; set; }
        public string desc { get; set; }
        public string validity { get; set; }
        public string last_update { get; set; }
    }

    public class FULLTT
    {
        public string rs { get; set; }
        public string desc { get; set; }
        public string validity { get; set; }
        public string last_update { get; set; }
    }


    public class TOPUP
    {
        public string rs { get; set; }
        public string desc { get; set; }
        public string validity { get; set; }
        public string last_update { get; set; }
    }
}