using YESBankCore.Controller;

namespace YesBankCore.Response.MoneyTransfer
{
    internal class StatusCheckAPIResponse
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
        public TransferStatusData data { get; set; }

        public List<StatusErrorResponse> errorResponse { get; set; }
    }

    public class StatusErrorResponse
    {
        private string _errorMsg;

        public string errorMsg
        {
            get => _errorMsg.TrimIfNotNull();
            set => _errorMsg = value.TrimIfNotNull();
        }

    }

    public class TransferStatusData
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

        private string _sorTxnId;

        public string sorTxnId
        {
            get => _sorTxnId.TrimIfNotNull();
            set => _sorTxnId = value.TrimIfNotNull();
        }

        private string _isRefund;

        public string isRefund
        {
            get => _isRefund.TrimIfNotNull();
            set => _isRefund = value.TrimIfNotNull();
        }
    }
}
