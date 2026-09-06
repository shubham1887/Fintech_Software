using Project.Processing;

namespace Project.Requests.User
{
    public class UpdateIsSecureIPRequest
    {
        private bool _IsIPSecure;

        public bool IsIPSecure { get => _IsIPSecure; set => _IsIPSecure = value; }
        private string _OTP;
        public string OTP { get => _OTP.TrimIfNotNull(); set => _OTP = value.TrimIfNotNull(); }
    }
}