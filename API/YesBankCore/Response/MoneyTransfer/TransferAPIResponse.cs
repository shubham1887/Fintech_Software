using YESBankCore.Controller;

namespace YesBankCore.Response.MoneyTransfer
{
    internal class TransferAPIResponse
    {
        private string _responseMsg;

        public string responseMsg
        {
            get => _responseMsg.TrimIfNotNull();
            set => _responseMsg = value.TrimIfNotNull();
        }

        private string _txnId;

        public string txnId
        {
            get => _txnId.TrimIfNotNull();
            set => _txnId = value.TrimIfNotNull();
        }

        public Data data { get; set; }

        public List<TransferErrorResponse> errorResponse { get; set; }
    }

    public class TransferErrorResponse
    {

        private string _errorMsg;

        public string errorMsg
        {
            get => _errorMsg.TrimIfNotNull();
            set => _errorMsg = value.TrimIfNotNull();
        }
    }

    public class Data
    {
        private string _beneficiaryName;

        public string beneficiaryName
        {
            get => _beneficiaryName.TrimIfNotNull();
            set => _beneficiaryName = value.TrimIfNotNull();
        }

        private string _rrn;

        public string rrn
        {
            get => _rrn.TrimIfNotNull();
            set => _rrn = value.TrimIfNotNull();
        }

        private string _transferAmount;

        public string transferAmount
        {
            get => _transferAmount.TrimIfNotNull();
            set => _transferAmount = value.TrimIfNotNull();
        }

        private string _paymentMode;

        public string paymentMode
        {
            get => _paymentMode.TrimIfNotNull();
            set => _paymentMode = value.TrimIfNotNull();
        }

        private string _transactionDate;

        public string transactionDate
        {
            get => _transactionDate.TrimIfNotNull();
            set => _transactionDate = value.TrimIfNotNull();
        }
    }
}
