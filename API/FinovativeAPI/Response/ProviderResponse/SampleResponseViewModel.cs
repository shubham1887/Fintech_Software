using Project.Processing;

namespace Project.Response.ProviderResponse
{
    public class SampleResponseViewModel
    {
        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _Type;

        public string Type
        {
            get => _Type.TrimIfNotNull();
            set => _Type = value.TrimIfNotNull();
        }

        private string _TextMustExit;

        public string TextMustExit
        {
            get => _TextMustExit.TrimIfNotNull();
            set => _TextMustExit = value.TrimIfNotNull();
        }

        private string _Response;

        public string Response
        {
            get => _Response.TrimIfNotNull();
            set => _Response = value.TrimIfNotNull();
        }

        private string _DeveloperName;

        public string DeveloperName
        {
            get => _DeveloperName.TrimIfNotNull();
            set => _DeveloperName = value.TrimIfNotNull();
        }
    }
}