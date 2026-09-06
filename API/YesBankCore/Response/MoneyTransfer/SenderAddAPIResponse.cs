using YESBankCore.Controller;

namespace YesBankCore.Response.MoneyTransfer
{
    internal class SenderAddAPIResponse
    {
        private string _responseMsg;

        public string responseMsg
        {
            get => _responseMsg.TrimIfNotNull();
            set => _responseMsg = value.TrimIfNotNull();
        }

        private string _responseCode;

        public string responseCode
        {
            get => _responseCode.TrimIfNotNull();
            set => _responseCode = value.TrimIfNotNull();
        }

        public List<SenderAddErrorResponse> errorResponse { get; set; }
    }

    public class SenderAddErrorResponse
    {

        private string _errorMsg;

        public string errorMsg
        {
            get => _errorMsg.TrimIfNotNull();
            set => _errorMsg = value.TrimIfNotNull();
        }
    }
}
