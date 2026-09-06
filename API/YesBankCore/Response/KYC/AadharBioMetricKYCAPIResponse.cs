using YESBankCore.Controller;

namespace YesBankCore.Response.KYC
{
    internal class AadharBioMetricKYCAPIResponse
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

        private string _uidToken;

        public string uidToken
        {
            get => _uidToken.TrimIfNotNull();
            set => _uidToken = value.TrimIfNotNull();
        }

        private string _aadhaarName;

        public string aadhaarName
        {
            get => _aadhaarName.TrimIfNotNull();
            set => _aadhaarName = value.TrimIfNotNull();
        }

        private string _address;

        public string address
        {
            get => _address.TrimIfNotNull();
            set => _address = value.TrimIfNotNull();
        }      
    }
}
