using Project.Processing;

namespace Project.Response.MicroATM
{
    public class PaySwiffStatusCheckResponse
    {
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

        private string _referenceNumber;

        public string referenceNumber
        {
            get => _referenceNumber.TrimIfNotNull();
            set => _referenceNumber = value.TrimIfNotNull();
        }

        private string _amount;

        public string amount
        {
            get => _amount.TrimIfNotNull();
            set => _amount = value.TrimIfNotNull();
        }

        private string _rrn;

        public string rrn
        {
            get => _rrn.TrimIfNotNull();
            set => _rrn = value.TrimIfNotNull();
        }

        private string _transactionStatus;

        public string transactionStatus
        {
            get => _transactionStatus.TrimIfNotNull();
            set => _transactionStatus = value.TrimIfNotNull();
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

        private int _merchantId;

        public int merchantId
        {
            get => _merchantId;
            set => _merchantId = value;
        }

        private string _merchantRefNo;

        public string merchantRefNo
        {
            get => _merchantRefNo.TrimIfNotNull();
            set => _merchantRefNo = value.TrimIfNotNull();
        }

        private string _cardHolderName;

        public string cardHolderName
        {
            get => _cardHolderName.TrimIfNotNull();
            set => _cardHolderName = value.TrimIfNotNull();
        }        

        private string _paymentMethod;

        public string paymentMethod
        {
            get => _paymentMethod.TrimIfNotNull();
            set => _paymentMethod = value.TrimIfNotNull();
        }

        private string _processorName;

        public string processorName
        {
            get => _processorName.TrimIfNotNull();
            set => _processorName = value.TrimIfNotNull();
        }     
    }
}