using Project.Processing;

namespace Project.Response.MDMTAPIResponse
{
    public class SenderRegistrationAPIResponse
    {
        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _OTPRefID;

        public string OTPRefID
        {
            get => _OTPRefID.TrimIfNotNull();
            set => _OTPRefID = value.TrimIfNotNull();
        }

        private string _SenderID;

        public string SenderID
        {
            get => _SenderID.TrimIfNotNull();
            set => _SenderID = value.TrimIfNotNull();
        }
    }
}