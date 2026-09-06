using Project.Processing;

namespace Project.Response.MicroATM
{
    public class PaySwiffCallbackResponse
    {
        private string _amount;

        public string amount
        {
            get => _amount.TrimIfNotNull();
            set => _amount = value.TrimIfNotNull();
        }

        private string _authId;

        public string authId
        {
            get => _authId.TrimIfNotNull();
            set => _authId = value.TrimIfNotNull();
        }

        private string _cardBrand;

        public string cardBrand
        {
            get => _cardBrand.TrimIfNotNull();
            set => _cardBrand = value.TrimIfNotNull();
        }

        private string _cardHolderName;

        public string cardHolderName
        {
            get => _cardHolderName.TrimIfNotNull();
            set => _cardHolderName = value.TrimIfNotNull();
        }

        private string _cardNumber;

        public string cardNumber
        {
            get => _cardNumber.TrimIfNotNull();
            set => _cardNumber = value.TrimIfNotNull();
        }

        private string _cardType;

        public string cardType
        {
            get => _cardType.TrimIfNotNull();
            set => _cardType = value.TrimIfNotNull();
        }

        private string _merchantId;

        public string merchantId
        {
            get => _merchantId.TrimIfNotNull();
            set => _merchantId = value.TrimIfNotNull();
        }

        private string _merchantRefInvoiceNo;

        public string merchantRefInvoiceNo
        {
            get => _merchantRefInvoiceNo.TrimIfNotNull();
            set => _merchantRefInvoiceNo = value.TrimIfNotNull();
        }

        private string _processorName;

        public string processorName
        {
            get => _processorName.TrimIfNotNull();
            set => _processorName = value.TrimIfNotNull();
        }

        private string _responseCode;

        public string responseCode
        {
            get => _responseCode.TrimIfNotNull();
            set => _responseCode = value.TrimIfNotNull();
        }

        private string _responseMessage;

        public string responseMessage
        {
            get => _responseMessage.TrimIfNotNull();
            set => _responseMessage = value.TrimIfNotNull();
        }

        private string _rrn;

        public string rrn
        {
            get => _rrn.TrimIfNotNull();
            set => _rrn = value.TrimIfNotNull();
        }

        private string _secureHash;

        public string secureHash
        {
            get => _secureHash.TrimIfNotNull();
            set => _secureHash = value.TrimIfNotNull();
        }

        private string _status;

        public string status
        {
            get => _status.TrimIfNotNull();
            set => _status = value.TrimIfNotNull();
        }

        private string _transactionId;

        public string transactionId
        {
            get => _transactionId.TrimIfNotNull();
            set => _transactionId = value.TrimIfNotNull();
        }

        private string _terminalId;

        public string terminalId
        {
            get => _terminalId.TrimIfNotNull();
            set => _terminalId = value.TrimIfNotNull();
        }

        private string _transactionType;

        public string transactionType
        {
            get => _transactionType.TrimIfNotNull();
            set => _transactionType = value.TrimIfNotNull();
        }
    }
}