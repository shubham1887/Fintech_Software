using YESBankCore.Controller;

namespace YesBankCore.Response.MoneyTransfer
{
    internal class VerifyStatusCheckAPIResponse
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

        public VerifyStatusData data { get; set; }

    }

    public class VerifyStatusData
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

        private string _isRefund;

        public string isRefund
        {
            get => _isRefund.TrimIfNotNull();
            set => _isRefund = value.TrimIfNotNull();
        }
    }
}
