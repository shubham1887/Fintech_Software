using Project.Processing;

namespace Project.Response.PaymentGateway
{
    public class UPIGatewayOrderResponse
    {
        public bool status { get; set; }

        private string _msg;

        public string msg
        {
            get => _msg.TrimIfNotNull();
            set => _msg = value.TrimIfNotNull();
        }

        public UPIGatewayData data { get; set; }
    }

    public class UPIGatewayData
    {
        private string _payment_url;

        public string payment_url
        {
            get => _payment_url.TrimIfNotNull();
            set => _payment_url = value.TrimIfNotNull();
        }

        public int order_id { get; set; }
    }
}