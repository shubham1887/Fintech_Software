using YESBankCore.Controller;

namespace YesBankCore.Response.KYC
{
    public class CreateAgentResponse
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
        
        private string _OTPToken;

        public string OTPToken
        {
            get => _OTPToken.TrimIfNotNull();
            set => _OTPToken = value.TrimIfNotNull();
        }
        private string _RequestID;

        public string RequestID
        {
            get => _RequestID.TrimIfNotNull();
            set => _RequestID = value.TrimIfNotNull();
        }
    }
}
