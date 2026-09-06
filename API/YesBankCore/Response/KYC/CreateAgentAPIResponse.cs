using YESBankCore.Controller;

namespace YesBankCore.Response.KYC
{
    internal class CreateAgentAPIResponse
    {
        private string _requestid;

        public string requestid
        {
            get => _requestid.TrimIfNotNull();
            set => _requestid = value.TrimIfNotNull();
        }

        private string _status;

        public string status
        {
            get => _status.TrimIfNotNull();
            set => _status = value.TrimIfNotNull();
        }

        private string _responseMessage;

        public string responseMessage
        {
            get => _responseMessage.TrimIfNotNull();
            set => _responseMessage = value.TrimIfNotNull();
        }

        private string _otpToken;

        public string otpToken
        {
            get => _otpToken.TrimIfNotNull();
            set => _otpToken = value.TrimIfNotNull();
        }
    }
}
