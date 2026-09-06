using Project.Processing;

namespace Project.Response.User
{
    public class BeneficialUserViewModel
    {
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

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private int _WalletID;

        public int WalletID
        {
            get => _WalletID;
            set => _WalletID = value;
        }

        private int _ReceiverID;

        public int ReceiverID
        {
            get => _ReceiverID;
            set => _ReceiverID = value;
        }

        private string _ReferralCode;

        public string ReferralCode
        {
            get => _ReferralCode.TrimIfNotNull();
            set => _ReferralCode = value.TrimIfNotNull();
        }

        private string _Receiver;

        public string Receiver
        {
            get => _Receiver.TrimIfNotNull();
            set => _Receiver = value.TrimIfNotNull();
        }

        private string _ReferralUser;

        public string ReferralUser
        {
            get => _ReferralUser.TrimIfNotNull();
            set => _ReferralUser = value.TrimIfNotNull();
        }
        private string _ReferralCreatedUserID;

        public string ReferralCreatedUserID
        {
            get => _ReferralCreatedUserID.TrimIfNotNull();
            set => _ReferralCreatedUserID = value.TrimIfNotNull();
        }
        
    }
}