using Project.Processing;

namespace Project.Response.Quickekyc
{
    public class AadharGenerateOtpResponse
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

        private string _ClientID;

        public string ClientID
        {
            get => _ClientID.TrimIfNotNull();
            set => _ClientID = value.TrimIfNotNull();
        }
    }
}