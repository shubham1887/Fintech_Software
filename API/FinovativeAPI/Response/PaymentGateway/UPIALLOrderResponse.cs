using Project.Processing;

namespace Project.Response.PaymentGateway
{
    public class UPIALLOrderResponse
    {
        public bool status { get; set; }

        private string _message;

        public string message
        {
            get => _message.TrimIfNotNull();
            set => _message = value.TrimIfNotNull();
        }

        public UPIAllData result { get; set; }

        public UPIALLOrderResponse()
        {
            result = new UPIAllData();
        }

        //  public List<UPIAllData> result { get; set; } = new List<UPIAllData>();
    }

    public class UPIAllData
    {
        private string _payment_url;

        public string payment_url
        {
            get => _payment_url.TrimIfNotNull();
            set => _payment_url = value.TrimIfNotNull();
        }

        private string _orderId;

        public string orderId
        {
            get => _orderId.TrimIfNotNull();
            set => _orderId = value.TrimIfNotNull();
        }
    }
}