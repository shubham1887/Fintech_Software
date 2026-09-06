using YESBankCore.Controller;

namespace YesBankCore.Response.MoneyTransfer
{
    internal class VerifyAPIResponse
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
        public VerifyData data { get; set; }

        public List<VerifyErrorResponse> errorResponse { get; set; }
    }

    public class VerifyErrorResponse
    {

        private string _errorMsg;

        public string errorMsg
        {
            get => _errorMsg.TrimIfNotNull();
            set => _errorMsg = value.TrimIfNotNull();
        }
    }

    public class VerifyData
    {
        private string _sorTxnId;

        public string sorTxnId
        {
            get => _sorTxnId.TrimIfNotNull();
            set => _sorTxnId = value.TrimIfNotNull();
        }

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
    }
}
