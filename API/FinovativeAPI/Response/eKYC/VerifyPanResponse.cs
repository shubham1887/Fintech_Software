using Project.Processing;

namespace Project.Response.eKYC
{
    public class VerifyPanResponse
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

        private string _FullName;

        public string FullName
        {
            get => _FullName.TrimIfNotNull();
            set => _FullName = value.TrimIfNotNull();
        }
    }
}