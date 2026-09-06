using Project.Processing;

namespace Project.Response.SDK
{
    public class GenerateURLResponse
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

        private string _RedirectURL;

        public string RedirectURL
        {
            get => _RedirectURL.TrimIfNotNull();
            set => _RedirectURL = value.TrimIfNotNull();
        }

        private string _SecureToken;

        public string SecureToken
        {
            get => _SecureToken.TrimIfNotNull();
            set => _SecureToken = value.TrimIfNotNull();
        }
    }
}