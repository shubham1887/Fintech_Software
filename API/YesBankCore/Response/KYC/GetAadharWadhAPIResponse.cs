using YESBankCore.Controller;

namespace YesBankCore.Response.KYC
{
    internal class GetAadharWadhAPIResponse
    {
        private string _requestid;

        public string requestid
        {
            get => _requestid.TrimIfNotNull();
            set => _requestid = value.TrimIfNotNull();
        }

        private string _status;

        public string status
        {
            get => _status.TrimIfNotNull();
            set => _status = value.TrimIfNotNull();
        }

        private string _responseMessage;

        public string responseMessage
        {
            get => _responseMessage.TrimIfNotNull();
            set => _responseMessage = value.TrimIfNotNull();
        }

        private string _wadh;

        public string wadh
        {
            get => _wadh.TrimIfNotNull();
            set => _wadh = value.TrimIfNotNull();
        }
    }
}
