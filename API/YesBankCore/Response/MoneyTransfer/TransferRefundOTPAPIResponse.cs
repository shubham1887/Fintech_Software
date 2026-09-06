using YESBankCore.Controller;

namespace YesBankCore.Response.MoneyTransfer
{
    internal class TransferRefundOTPAPIResponse
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

        public List<RefundErrorResponse> errorResponse { get; set; }
    }

    public class RefundErrorResponse
    {
        private string _errorMsg;

        public string errorMsg
        {
            get => _errorMsg.TrimIfNotNull();
            set => _errorMsg = value.TrimIfNotNull();
        }

    }
}
