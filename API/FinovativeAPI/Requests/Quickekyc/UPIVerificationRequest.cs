using Project.Processing;

namespace Project.Requests.Quickekyc
{
    public class UPIVerificationRequest
    {
        private string _key;

        public string key
        {
            get => _key.TrimIfNotNull();
            set => _key = value.TrimIfNotNull();
        }

        private string _upi_id;

        public string upi_id
        {
            get => _upi_id.TrimIfNotNull();
            set => _upi_id = value.TrimIfNotNull();
        }

    }
}