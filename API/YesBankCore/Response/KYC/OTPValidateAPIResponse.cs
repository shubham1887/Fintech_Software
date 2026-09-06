using YESBankCore.Controller;

namespace YesBankCore.Response.KYC
{
    internal class OTPValidateAPIResponse
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

        private string _kycToken;

        public string kycToken
        {
            get => _kycToken.TrimIfNotNull();
            set => _kycToken = value.TrimIfNotNull();
        }
    }
}
