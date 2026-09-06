using Newtonsoft.Json;
using Project.Processing;

namespace Project.Response.PaysprintUPICollection
{
    public class UPICollectionCallbackResponse
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

        private string _param_enc;

        [JsonProperty("param_enc")]
        public string param_enc
        {
            get
            {
                return _param_enc.TrimIfNotNull();
            }
            set
            {
                _param_enc = value.TrimIfNotNull();
            }
        }

        public ParamOnboardingCallback param { get; set; }
    }

    public class ParamOnboardingCallback
    {
        public bool status { get; set; }
        public int response_code { get; set; }
        public int txnstatus { get; set; }

        public int api_type { get; set; }

        //    private string _api_type;
        //    [JsonProperty("api_type")]
        //    public string api_type
        //{
        //        get
        //        {
        //            return _api_type.TrimIfNotNull();
        //        }
        //        set
        //        {
        //        _api_type = value.TrimIfNotNull();
        //        }
        //    }

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

        private string _ttype;

        [JsonProperty("ttype")]
        public string ttype
        {
            get
            {
                return _ttype.TrimIfNotNull();
            }
            set
            {
                _ttype = value.TrimIfNotNull();
            }
        }

        private string _upi_txn_id;

        [JsonProperty("upi_txn_id")]
        public string upi_txn_id
        {
            get
            {
                return _upi_txn_id.TrimIfNotNull();
            }
            set
            {
                _upi_txn_id = value.TrimIfNotNull();
            }
        }

        private string _txn_completion_date;

        [JsonProperty("txn_completion_date")]
        public string txn_completion_date
        {
            get
            {
                return _txn_completion_date.TrimIfNotNull();
            }
            set
            {
                _txn_completion_date = value.TrimIfNotNull();
            }
        }

        private string _customer_virtual_address;

        [JsonProperty("customer_virtual_address")]
        public string customer_virtual_address
        {
            get
            {
                return _customer_virtual_address.TrimIfNotNull();
            }
            set
            {
                _customer_virtual_address = value.TrimIfNotNull();
            }
        }

        private string _customer_mobile_number;

        [JsonProperty("customer_mobile_number")]
        public string customer_mobile_number
        {
            get
            {
                return _customer_mobile_number.TrimIfNotNull();
            }
            set
            {
                _customer_mobile_number = value.TrimIfNotNull();
            }
        }

        private string _customer_account_name;

        [JsonProperty("customer_account_name")]
        public string customer_account_name
        {
            get
            {
                return _customer_account_name.TrimIfNotNull();
            }
            set
            {
                _customer_account_name = value.TrimIfNotNull();
            }
        }
    }
}