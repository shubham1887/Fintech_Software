using YESBankCore.Controller;

namespace YesBankCore.Response.MoneyTransfer
{
    internal class TransferRefundAPIResponse
    {
        private string _responseMsg;

        public string responseMsg
        {
            get => _responseMsg.TrimIfNotNull();
            set => _responseMsg = value.TrimIfNotNull();
        }

        public TransferRefundData data { get; set; }
    }

    public class TransferRefundData
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
    }
}
