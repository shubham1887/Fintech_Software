using Project.Processing;

namespace Project.Response.eKYCLog
{
    public class ViewModel
    {
        private int _UserID;

        public int UserID { get => _UserID; set => _UserID = value; }

        private string _DocumentType;

        public string DocumentType
        {
            get => _DocumentType.TrimIfNotNull();
            set => _DocumentType = value.TrimIfNotNull();
        }

        private string _Response;

        public string Response
        {
            get => _Response.TrimIfNotNull();
            set => _Response = value.TrimIfNotNull();
        }
    }
}