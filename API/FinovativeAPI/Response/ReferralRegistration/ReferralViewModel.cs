using Project.Processing;

namespace Project.Response.ReferralRegistration
{
    public class ReferralViewModel
    {
        private string _ReferralURL;

        public string ReferralURL
        {
            get => _ReferralURL.TrimIfNotNull();
            set => _ReferralURL = value.TrimIfNotNull();
        }

        private string _ReferralCode;

        public string ReferralCode
        {
            get => _ReferralCode.TrimIfNotNull();
            set => _ReferralCode = value.TrimIfNotNull();
        }

        private decimal _ReferralAmount;

        public decimal ReferralAmount
        {
            get => _ReferralAmount;
            set => _ReferralAmount = value;
        }

        private decimal _ReferralBenefitMinAmount;

        public decimal ReferralBenefitMinAmount
        {
            get => _ReferralBenefitMinAmount;
            set => _ReferralBenefitMinAmount = value;
        }
    }
}