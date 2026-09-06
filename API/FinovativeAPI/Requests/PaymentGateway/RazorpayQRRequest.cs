using Newtonsoft.Json;
using Project.Processing;

namespace Project.Requests.PaymentGateway
{
    public class RazorpayQRRequest
    {
        private string _type;

        [JsonProperty("type")]
        public string type
        {
            get
            {
                return _type.TrimIfNotNull();
            }
            set
            {
                _type = value.TrimIfNotNull();
            }
        }

        private string _usage;

        [JsonProperty("usage")]
        public string usage
        {
            get
            {
                return _usage.TrimIfNotNull();
            }
            set
            {
                _usage = value.TrimIfNotNull();
            }
        }

        private bool _fixed_amount;

        [JsonProperty("fixed_amount")]
        public bool fixed_amount
        {
            get
            {
                return _fixed_amount;
            }
            set
            {
                _fixed_amount = value;
            }
        }

        private int _payment_amount;

        [JsonProperty("payment_amount")]
        public int payment_amount
        {
            get => _payment_amount;
            set => _payment_amount = value;
        }

        private long _close_by;

        [JsonProperty("close_by")]
        public long close_by
        {
            get => _close_by;
            set => _close_by = value;
        }
    }
}