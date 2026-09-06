using Project.Processing;

namespace Project.Requests.PaymentGateway
{
    public class UPIALLCreateOrderRequest
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

        private string _txnAmount;

        public string txnAmount
        {
            get => _txnAmount.TrimIfNotNull();
            set => _txnAmount = value.TrimIfNotNull();
        }

        private string _txnNote;

        public string txnNote
        {
            get => _txnNote.TrimIfNotNull();
            set => _txnNote = value.TrimIfNotNull();
        }

        private string _customerName;

        public string customerName
        {
            get => _customerName.TrimIfNotNull();
            set => _customerName = value.TrimIfNotNull();
        }

        private string _customerEmail;

        public string customerEmail
        {
            get => _customerEmail.TrimIfNotNull();
            set => _customerEmail = value.TrimIfNotNull();
        }

        private string _customerMobile;

        public string customerMobile
        {
            get => _customerMobile.TrimIfNotNull();
            set => _customerMobile = value.TrimIfNotNull();
        }

        private string _callbackUrl;

        public string callbackUrl
        {
            get => _callbackUrl.TrimIfNotNull();
            set => _callbackUrl = value.TrimIfNotNull();
        }
    }
}