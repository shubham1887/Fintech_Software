using Newtonsoft.Json;
using Project.Processing;

namespace Project.Response.CredoPay
{
    public class TerminalStatusResponse
    {
        private string _mobile;
        public string mobile
        {
            get => _mobile.TrimIfNotNull();
            set => _mobile = value.TrimIfNotNull();
        }

        private string _cpId;
        public string cpId
        {
            get => _cpId.TrimIfNotNull();
            set => _cpId = value.TrimIfNotNull();
        }
        private string _status;
        public string status
        {
            get => _status.TrimIfNotNull();
            set => _status = value.TrimIfNotNull();
        }
    }
}
