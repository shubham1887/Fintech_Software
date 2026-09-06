using YESBankCore.Controller;

namespace YesBankCore.Response.KYC
{
    public class OTPValidateResponse
    {
        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private string _KYCToken;

        public string KYCToken
        {
            get => _KYCToken.TrimIfNotNull();
            set => _KYCToken = value.TrimIfNotNull();
        }
    }
}
