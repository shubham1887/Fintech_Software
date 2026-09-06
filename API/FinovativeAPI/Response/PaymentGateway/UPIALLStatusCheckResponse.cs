using Project.Processing;

namespace Project.Response.PaymentGateway
{
    public class UPIALLStatusCheckResponse
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

        public UPIAllDataStatus result { get; set; }
    }

    public class UPIAllDataStatus
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

        private string _resultInfo;

        public string resultInfo
        {
            get => _resultInfo.TrimIfNotNull();
            set => _resultInfo = value.TrimIfNotNull();
        }

        private string _txnStatus;

        public string txnStatus
        {
            get => _txnStatus.TrimIfNotNull();
            set => _txnStatus = value.TrimIfNotNull();
        }

        private string _txnAmount;

        public string txnAmount
        {
            get => _txnAmount.TrimIfNotNull();
            set => _txnAmount = value.TrimIfNotNull();
        }

        private string _txnId;

        public string txnId
        {
            get => _txnId.TrimIfNotNull();
            set => _txnId = value.TrimIfNotNull();
        }

        private string _bankTxnId;

        public string bankTxnId
        {
            get => _bankTxnId.TrimIfNotNull();
            set => _bankTxnId = value.TrimIfNotNull();
        }

        private string _paymentMode;

        public string paymentMode
        {
            get => _paymentMode.TrimIfNotNull();
            set => _paymentMode = value.TrimIfNotNull();
        }

        private string _txnDate;

        public string txnDate
        {
            get => _txnDate.TrimIfNotNull();
            set => _txnDate = value.TrimIfNotNull();
        }

        private string _utr;

        public string utr
        {
            get => _utr.TrimIfNotNull();
            set => _utr = value.TrimIfNotNull();
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

        private string _customerUpi;

        public string customerUpi
        {
            get => _customerUpi.TrimIfNotNull();
            set => _customerUpi = value.TrimIfNotNull();
        }

        private string _txnNote;

        public string txnNote
        {
            get => _txnNote.TrimIfNotNull();
            set => _txnNote = value.TrimIfNotNull();
        }

        private string _payee_vpa;

        public string payee_vpa
        {
            get => _payee_vpa.TrimIfNotNull();
            set => _payee_vpa = value.TrimIfNotNull();
        }
    }
}