using Project.Processing;

namespace Project.Response.AEPS
{
    public class PaytmAEPSCallbackResponse
    {
        private string _orderId;

        public string orderId
        {
            get => _orderId.TrimIfNotNull();
            set => _orderId = value.TrimIfNotNull();
        }

        private string _amount;

        public string amount
        {
            get => _amount.TrimIfNotNull();
            set => _amount = value.TrimIfNotNull();
        }

        private string _agentCustId;

        public string agentCustId
        {
            get => _agentCustId.TrimIfNotNull();
            set => _agentCustId = value.TrimIfNotNull();
        }

        private string _status;

        public string status
        {
            get => _status.TrimIfNotNull();
            set => _status = value.TrimIfNotNull();
        }

        private string _responseMessage;

        public string responseMessage
        {
            get => _responseMessage.TrimIfNotNull();
            set => _responseMessage = value.TrimIfNotNull();
        }

        private string _created;

        public string created
        {
            get => _created.TrimIfNotNull();
            set => _created = value.TrimIfNotNull();
        }

        private string _depositorMobileNumber;

        public string depositorMobileNumber
        {
            get => _depositorMobileNumber.TrimIfNotNull();
            set => _depositorMobileNumber = value.TrimIfNotNull();
        }

        private string _requestReferenceId;

        public string requestReferenceId
        {
            get => _requestReferenceId.TrimIfNotNull();
            set => _requestReferenceId = value.TrimIfNotNull();
        }

        private string _productType;

        public string productType
        {
            get => _productType.TrimIfNotNull();
            set => _productType = value.TrimIfNotNull();
        }

        private string _terminalCode;

        public string terminalCode
        {
            get => _terminalCode.TrimIfNotNull();
            set => _terminalCode = value.TrimIfNotNull();
        }

        private string _failureCode;

        public string failureCode
        {
            get => _failureCode.TrimIfNotNull();
            set => _failureCode = value.TrimIfNotNull();
        }
    }
}