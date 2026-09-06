using Project.Processing;

namespace Project.Response.PaytmCCFAPIResponse
{
    public class AddBeneficiaryAPIResponse
    {
        private string _status;
        public string status
        {
            get => _status.TrimIfNotNull();
            set => _status = value.TrimIfNotNull();
        }

        private string _response_code;
        public string response_code
        {
            get => _response_code.TrimIfNotNull();
            set => _response_code = value.TrimIfNotNull();
        }

        private string _message;
        public string message
        {
            get => _message.TrimIfNotNull();
            set => _message = value.TrimIfNotNull();
        }


        private string _beneficiaryId;
        public string beneficiaryId
        {
            get => _beneficiaryId.TrimIfNotNull();
            set => _beneficiaryId = value.TrimIfNotNull();
        }

    }
}
