using Project.Processing;

namespace Project.Response.PanCard
{
    public class Paysprint_GenerateURLResponse
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

        private string _URL;

        public string URL
        {
            get => _URL.TrimIfNotNull();
            set => _URL = value.TrimIfNotNull();
        }

        private string _EncData;

        public string EncData
        {
            get => _EncData.TrimIfNotNull();
            set => _EncData = value.TrimIfNotNull();
        }
    }
}