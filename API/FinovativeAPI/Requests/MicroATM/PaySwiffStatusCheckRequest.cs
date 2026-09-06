using Project.Processing;

namespace Project.Requests.MicroATM
{
    public class PaySwiffStatusCheckRequest
    {
        private string _merchantAPIKey;

        public string merchantAPIKey
        {
            get => _merchantAPIKey.TrimIfNotNull();
            set => _merchantAPIKey = value.TrimIfNotNull();
        }

        private string _partnerAPIKey;

        public string partnerAPIKey
        {
            get => _partnerAPIKey.TrimIfNotNull();
            set => _partnerAPIKey = value.TrimIfNotNull();
        }

        private string _transcationType;

        public string transcationType
        {
            get => _transcationType.TrimIfNotNull();
            set => _transcationType = value.TrimIfNotNull();
        }

        private string _merchantRefNo;

        public string merchantRefNo
        {
            get => _merchantRefNo.TrimIfNotNull();
            set => _merchantRefNo = value.TrimIfNotNull();
        }

    }
}