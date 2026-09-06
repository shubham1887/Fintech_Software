using Project.Processing;

namespace Project.Response
{
    public class IServeU_CallbackReturnResponse
    {
        private string _status;
        public string status
        {
            get => _status.TrimIfNotNull();
            set => _status = value.TrimIfNotNull();
        }
        private string _statusDesc;

        public string statusDesc
        {
            get => _statusDesc.TrimIfNotNull();
            set => _statusDesc = value.TrimIfNotNull();
        }
    }
}
