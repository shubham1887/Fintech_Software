using Project.Processing;

namespace Project.Requests.PaymentGateway
{
    public class UPIALLStatusCheckRequest
    {
        private string _token;

        public string token
        {
            get => _token.TrimIfNotNull();
            set => _token = value.TrimIfNotNull();
        }

        private string _orderId;

        public string orderId
        {
            get => _orderId.TrimIfNotNull();
            set => _orderId = value.TrimIfNotNull();
        }
    }
}