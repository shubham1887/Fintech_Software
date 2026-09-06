using Project.Processing;

namespace Project.Response.APILog
{
    public class APIUserCallbackViewModel
    {
        private string _DOC;

        public string DOC
        {
            get => _DOC.TrimIfNotNull();
            set => _DOC = value.TrimIfNotNull();
        }

        private string _Request;

        public string Request
        {
            get => _Request.TrimIfNotNull();
            set => _Request = value.TrimIfNotNull();
        }

        private string _Response;

        public string Response
        {
            get => _Response.TrimIfNotNull();
            set => _Response = value.TrimIfNotNull();
        }
    }
}