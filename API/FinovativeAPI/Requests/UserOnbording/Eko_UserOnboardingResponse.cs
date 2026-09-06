using Project.Processing;

namespace Project.Requests.UserOnbording
{
    public class Eko_UserOnboardingResponse
    {
        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private string _UserCode;

        public string UserCode
        {
            get => _UserCode.TrimIfNotNull();
            set => _UserCode = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }
    }
}