using MplanCore.Controller;
using MplanCore.Response.ShighraPay;
using Newtonsoft.Json;

namespace MplanCore.Response
{
    public class InfoAPIPlan
    {
        private string _status;

        public string status
        {
            get => _status.TrimIfNotNull();
            set => _status = value.TrimIfNotNull();
        }

        private string _message;

        public string message
        {
            get => _message.TrimIfNotNull();
            set => _message = value.TrimIfNotNull();
        }

        private string _cust;

        public string cust
        {
            get => _cust.TrimIfNotNull();
            set => _cust = value.TrimIfNotNull();
        }

        private string _operator;

        public string @operator
        {
            get => _operator.TrimIfNotNull();
            set => _operator = value.TrimIfNotNull();
        }

        public Data data { get; set; }
    }

    public class Data
    {
        [JsonProperty("cricket plans")]
        public List<CricketPlan> cricketplans { get; set; }

        [JsonProperty("truly unlimited")]
        public List<TrulyUnlimited> trulyunlimited { get; set; }

        [JsonProperty("data add on")]
        public List<DataAddOn> dataaddon { get; set; }

        [JsonProperty("top up")]
        public List<TopUp> topup { get; set; }

        [JsonProperty("international roaming")]
        public List<InternationalRoaming> internationalroaming { get; set; }
    }

    public class CricketPlan
    {
        public int amount { get; set; }
        public object talktime { get; set; }
        public int validity { get; set; }
        public string description { get; set; }
    }

    public class DataAddOn
    {
        public int amount { get; set; }
        public object talktime { get; set; }
        public int validity { get; set; }
        public string description { get; set; }
    }

    public class InternationalRoaming
    {
        public int amount { get; set; }
        public object talktime { get; set; }
        public int validity { get; set; }
        public string description { get; set; }
    }

    public class TopUp
    {
        public int amount { get; set; }
        public string talktime { get; set; }
        public int validity { get; set; }
        public string description { get; set; }
    }

    public class TrulyUnlimited
    {
        public int amount { get; set; }
        public string talktime { get; set; }
        public int validity { get; set; }
        public string description { get; set; }
    }

}