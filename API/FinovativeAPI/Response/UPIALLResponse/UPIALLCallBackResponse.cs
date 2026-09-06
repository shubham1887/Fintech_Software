using Project.Processing;

namespace FinovativeAPI.Response.UPIALLResponse
{
    public class UPIALLCallBackResponse
    {
        private string _message;

        public string message
        {
            get => _message.TrimIfNotNull();
            set => _message = value.TrimIfNotNull();
        }

        private string _status;

        public string status
        {
            get => _status.TrimIfNotNull();
            set => _status = value.TrimIfNotNull();
        }

        private string _hash;

        public string hash
        {
            get => _hash.TrimIfNotNull();
            set => _hash = value.TrimIfNotNull();
        }
    }
}