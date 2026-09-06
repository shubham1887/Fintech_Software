using Newtonsoft.Json;
using Project.Processing;

namespace Project.Response.BusBooking
{
    public class BusBookingCallbackResponse
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

        public Callback param { get; set; }
    }

    public class Callback
    {
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

        private string _base_fare;

        [JsonProperty("base_fare")]
        public string base_fare
        {
            get
            {
                return _base_fare.TrimIfNotNull();
            }
            set
            {
                _base_fare = value.TrimIfNotNull();
            }
        }

        private string _comm;

        [JsonProperty("comm")]
        public string comm
        {
            get
            {
                return _comm.TrimIfNotNull();
            }
            set
            {
                _comm = value.TrimIfNotNull();
            }
        }

        private string _tds;

        [JsonProperty("tds")]
        public string tds
        {
            get
            {
                return _tds.TrimIfNotNull();
            }
            set
            {
                _tds = value.TrimIfNotNull();
            }
        }

        private string _total_deduction;

        [JsonProperty("total_deduction")]
        public string total_deduction
        {
            get
            {
                return _total_deduction.TrimIfNotNull();
            }
            set
            {
                _total_deduction = value.TrimIfNotNull();
            }
        }

        private string _customer_mobile;

        [JsonProperty("customer_mobile")]
        public string customer_mobile
        {
            get
            {
                return _customer_mobile.TrimIfNotNull();
            }
            set
            {
                _customer_mobile = value.TrimIfNotNull();
            }
        }

        private string _refid;

        [JsonProperty("refid")]
        public string refid
        {
            get
            {
                return _refid.TrimIfNotNull();
            }
            set
            {
                _refid = value.TrimIfNotNull();
            }
        }

        private string _customer_email;

        [JsonProperty("customer_email")]
        public string customer_email
        {
            get
            {
                return _customer_email.TrimIfNotNull();
            }
            set
            {
                _customer_email = value.TrimIfNotNull();
            }
        }

        private string _blockKey;

        [JsonProperty("blockKey")]
        public string blockKey
        {
            get
            {
                return _blockKey.TrimIfNotNull();
            }
            set
            {
                _blockKey = value.TrimIfNotNull();
            }
        }
    }
}