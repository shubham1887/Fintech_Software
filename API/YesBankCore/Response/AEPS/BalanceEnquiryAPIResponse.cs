using YESBankCore.Controller;

namespace YesBankCore.Response.AEPS
{
    internal class BalanceEnquiryAPIResponse
    {
        private string _responseCode;

        public string responseCode
        {
            get => _responseCode.TrimIfNotNull();
            set => _responseCode = value.TrimIfNotNull();
        }

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
        public BalanceData data { get; set; }
        public List<ErrorResponseBE> errorResponse { get; set; }

    }

    public class ErrorResponseBE
    {
        public string errorMsg { get; set; }
    }

    public class BalanceData
    {
        private string _balance;

        public string balance
        {
            get => _balance.TrimIfNotNull();
            set => _balance = value.TrimIfNotNull();
        }

        private string _referenceNo;

        public string referenceNo
        {
            get => _referenceNo.TrimIfNotNull();
            set => _referenceNo = value.TrimIfNotNull();
        }

        private string _sorTxnId;

        public string sorTxnId
        {
            get => _sorTxnId.TrimIfNotNull();
            set => _sorTxnId = value.TrimIfNotNull();
        }
        private string _sysTraceNo;

        public string sysTraceNo
        {
            get => _sysTraceNo.TrimIfNotNull();
            set => _sysTraceNo = value.TrimIfNotNull();
        }
        private string _transactionDate;

        public string transactionDate
        {
            get => _transactionDate.TrimIfNotNull();
            set => _transactionDate = value.TrimIfNotNull();
        }
    }
}
