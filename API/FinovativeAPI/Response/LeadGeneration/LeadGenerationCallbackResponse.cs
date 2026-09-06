using Newtonsoft.Json;
using Project.Processing;

namespace Project.Response.LeadGeneration
{
    public class LeadGenerationCallbackResponse
    {
        private string _eventName;

        [JsonProperty("event")]
        public string eventName
        {
            get
            {
                return _eventName.TrimIfNotNull();
            }
            set
            {
                _eventName = value.TrimIfNotNull();
            }
        }

        public ParamOnboardingCallback param { get; set; }
    }

    public class ParamOnboardingCallback
    {
        public string status { get; set; }

        private string _message;

        [JsonProperty("message")]
        public string message
        {
            get
            {
                return _message.TrimIfNotNull();
            }
            set
            {
                _message = value.TrimIfNotNull();
            }
        }

        private string _ackno;

        [JsonProperty("ackno")]
        public string ackno
        {
            get
            {
                return _ackno.TrimIfNotNull();
            }
            set
            {
                _ackno = value.TrimIfNotNull();
            }
        }

        private string _amount;

        [JsonProperty("amount")]
        public string amount
        {
            get
            {
                return _amount.TrimIfNotNull();
            }
            set
            {
                _amount = value.TrimIfNotNull();
            }
        }

        private string _referenceid;

        [JsonProperty("referenceid")]
        public string referenceid
        {
            get
            {
                return _referenceid.TrimIfNotNull();
            }
            set
            {
                _referenceid = value.TrimIfNotNull();
            }
        }

        private string _network;

        [JsonProperty("network")]
        public string network
        {
            get
            {
                return _network.TrimIfNotNull();
            }
            set
            {
                _network = value.TrimIfNotNull();
            }
        }

        private string _card_number;

        [JsonProperty("card_number")]
        public string card_number
        {
            get
            {
                return _card_number.TrimIfNotNull();
            }
            set
            {
                _card_number = value.TrimIfNotNull();
            }
        }

        private string _utr;

        [JsonProperty("utr")]
        public string utr
        {
            get
            {
                return _utr.TrimIfNotNull();
            }
            set
            {
                _utr = value.TrimIfNotNull();
            }
        }
    }
}