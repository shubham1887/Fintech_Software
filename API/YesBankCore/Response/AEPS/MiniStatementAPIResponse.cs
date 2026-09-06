using YESBankCore.Controller;

namespace YesBankCore.Response.AEPS
{
    internal class MiniStatementAPIResponse
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

        public MiniStmtData data { get; set; }

        public List<ErrorResponseMS> errorResponse { get; set; }
    }

    public class ErrorResponseMS
    {
        public string errorMsg { get; set; }
    }

    public class MiniStmtData
    {
        private string _referenceNo;

        public string referenceNo
        {
            get => _referenceNo.TrimIfNotNull();
            set => _referenceNo = value.TrimIfNotNull();
        }

        private string _txnErrorMsg;

        public string txnErrorMsg
        {
            get => _txnErrorMsg.TrimIfNotNull();
            set => _txnErrorMsg = value.TrimIfNotNull();
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

        private string _miniStatement;

        public string miniStatement
        {
            get => _miniStatement.TrimIfNotNull();
            set => _miniStatement = value.TrimIfNotNull();
        }
    }
}
