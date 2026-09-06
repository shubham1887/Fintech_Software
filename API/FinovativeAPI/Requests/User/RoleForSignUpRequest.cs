using Project.Processing;

namespace Project.Requests.User
{
    public class RoleForSignUpRequest
    {
        private string _ReferralCode;

        public string ReferralCode
        {
            get => _ReferralCode.TrimIfNotNull();
            set => _ReferralCode = value.TrimIfNotNull();
        }
    }
}