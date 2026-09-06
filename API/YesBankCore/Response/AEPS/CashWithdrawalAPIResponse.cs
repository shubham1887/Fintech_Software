using YESBankCore.Controller;

namespace YesBankCore.Response.AEPS
{
    internal class CashWithdrawalAPIResponse
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

        public Data data { get; set; }

        public List<ErrorResponse2> errorResponse { get; set; }

    }

    public class Data
    {
        private string _referenceNo;

        public string referenceNo
        {
            get => _referenceNo.TrimIfNotNull();
            set => _referenceNo = value.TrimIfNotNull();
        }

        private string _balanceAmount;

        public string balanceAmount
        {
            get => _balanceAmount.TrimIfNotNull();
            set => _balanceAmount = value.TrimIfNotNull();
        }

        private string _transactionDate;

        public string transactionDate
        {
            get => _transactionDate.TrimIfNotNull();
            set => _transactionDate = value.TrimIfNotNull();
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
    }
    public class ErrorResponse2
    {
        public string errorMsg { get; set; }
    }

}
