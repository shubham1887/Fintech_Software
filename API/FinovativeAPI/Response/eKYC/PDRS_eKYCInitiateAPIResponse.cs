using Project.Processing;

namespace Project.Response.eKYC
{
    public class PDRS_eKYCInitiateAPIResponse
    {
        private string _status;

        public string status
        {
            get => _status.TrimIfNotNull();
            set => _status = value.TrimIfNotNull();
        }

        private string _message;

        public string message
        {
            get => _message.TrimIfNotNull();
            set => _message = value.TrimIfNotNull();
        }

        private string _client_id;

        public string client_id
        {
            get => _client_id.TrimIfNotNull();
            set => _client_id = value.TrimIfNotNull();
        }
    }
}