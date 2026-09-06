using Project.Processing;

namespace Project.Response.PaymentGateway
{
    public class CashFreeOrderAndroidResponse
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

        private string _cftoken;

        public string cftoken
        {
            get => _cftoken.TrimIfNotNull();
            set => _cftoken = value.TrimIfNotNull();
        }
    }
}