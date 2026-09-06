using Project.Processing;

namespace Project.Response.SmsMaster
{
    public class SmsURLResponse
    {
        private string _MethodType;

        public string MethodType
        {
            get => _MethodType.TrimIfNotNull();
            set => _MethodType = value.TrimIfNotNull();
        }

        private string _HeaderJson;

        public string HeaderJson
        {
            get => _HeaderJson.TrimIfNotNull();
            set => _HeaderJson = value.TrimIfNotNull();
        }

        private string _SmsURL;

        public string SmsURL
        {
            get => _SmsURL.TrimIfNotNull();
            set => _SmsURL = value.TrimIfNotNull();
        }

        private string _Message;

        public string Message
        {
            get => _SmsURL.TrimIfNotNull();
            set => _SmsURL = value.TrimIfNotNull();
        }
    }
}