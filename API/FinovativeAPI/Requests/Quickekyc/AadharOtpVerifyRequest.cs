using Project.Processing;

namespace Project.Requests.Quickekyc
{
    public class AadharOtpVerifyRequest
    {
        private string _key;

        public string key
        {
            get => _key.TrimIfNotNull();
            set => _key = value.TrimIfNotNull();
        }

        private string _request_id;

        public string request_id
        {
            get => _request_id.TrimIfNotNull();
            set => _request_id = value.TrimIfNotNull();
        }

        private string _otp;

        public string otp
        {
            get => _otp.TrimIfNotNull();
            set => _otp = value.TrimIfNotNull();
        }
    }
}