using Project.Processing;

namespace Project.Response.AEPS
{
    public class YesBank_AuthenticationResponse
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
        private string _AuthReferenceNo;

        public string AuthReferenceNo
        {
            get => _AuthReferenceNo.TrimIfNotNull();
            set => _AuthReferenceNo = value.TrimIfNotNull();
        }
    }
}