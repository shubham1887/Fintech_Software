using Project.Processing;

namespace Project.Response.AEPS
{
    public class EzyPayGenerateSessionResponse
    {
        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private string _SessionID;

        public string SessionID
        {
            get => _SessionID.TrimIfNotNull();
            set => _SessionID = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _LoginURL;

        public string LoginURL
        {
            get => _LoginURL.TrimIfNotNull();
            set => _LoginURL = value.TrimIfNotNull();
        }
    }
}