using Project.Processing;

namespace Project.Requests.Quickekyc
{
    public class VerificationRequest
    {
        private string _key;

        public string key
        {
            get => _key.TrimIfNotNull();
            set => _key = value.TrimIfNotNull();
        }

        private string _id_number;

        public string id_number
        {
            get => _id_number.TrimIfNotNull();
            set => _id_number = value.TrimIfNotNull();
        }
    }
}