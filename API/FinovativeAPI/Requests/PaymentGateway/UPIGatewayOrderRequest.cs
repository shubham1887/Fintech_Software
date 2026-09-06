using Project.Processing;

namespace Project.Requests.PaymentGateway
{
    public class UPIGatewayOrderRequest
    {
        private string _key;

        public string key
        {
            get => _key.TrimIfNotNull();
            set => _key = value.TrimIfNotNull();
        }

        private string _client_txn_id;

        public string client_txn_id
        {
            get => _client_txn_id.TrimIfNotNull();
            set => _client_txn_id = value.TrimIfNotNull();
        }

        private string _amount;

        public string amount
        {
            get => _amount.TrimIfNotNull();
            set => _amount = value.TrimIfNotNull();
        }

        private string _p_info;

        public string p_info
        {
            get => _p_info.TrimIfNotNull();
            set => _p_info = value.TrimIfNotNull();
        }

        private string _customer_name;

        public string customer_name
        {
            get => _customer_name.TrimIfNotNull();
            set => _customer_name = value.TrimIfNotNull();
        }

        private string _customer_email;

        public string customer_email
        {
            get => _customer_email.TrimIfNotNull();
            set => _customer_email = value.TrimIfNotNull();
        }

        private string _customer_mobile;

        public string customer_mobile
        {
            get => _customer_mobile.TrimIfNotNull();
            set => _customer_mobile = value.TrimIfNotNull();
        }

        private string _redirect_url;

        public string redirect_url
        {
            get => _redirect_url.TrimIfNotNull();
            set => _redirect_url = value.TrimIfNotNull();
        }

        private string _udf1;

        public string udf1
        {
            get => _udf1.TrimIfNotNull();
            set => _udf1 = value.TrimIfNotNull();
        }

        private string _udf2;

        public string udf2
        {
            get => _udf2.TrimIfNotNull();
            set => _udf2 = value.TrimIfNotNull();
        }

        private string _udf3;

        public string udf3
        {
            get => _udf3.TrimIfNotNull();
            set => _udf3 = value.TrimIfNotNull();
        }
    }
}