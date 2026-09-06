using Project.Processing;

namespace Project.Response.FingPay
{
    public class FingpayInsuranceCallbackResponse
    {
        private bool _status;

        public bool status
        {
            get => _status;
            set => _status = value;
        }
        
        private string _errorMessage;

        public string errorMessage
        {
            get => _errorMessage.TrimIfNotNull();
            set => _errorMessage = value.TrimIfNotNull();
        }

        private string _merchantTransactionId;

        public string merchantTransactionId
        {
            get => _merchantTransactionId.TrimIfNotNull();
            set => _merchantTransactionId = value.TrimIfNotNull();
        }

    }
}
