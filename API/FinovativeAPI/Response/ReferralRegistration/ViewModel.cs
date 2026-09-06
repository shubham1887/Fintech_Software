using Project.Processing;

namespace Project.Response.ReferralRegistration
{
    public class ViewModel
    {
        private int _ReferralRegistrationID;

        public int ReferralRegistrationID
        {
            get => _ReferralRegistrationID;
            set => _ReferralRegistrationID = value;
        }

        private int _CreatedByUserID;

        public int CreatedByUserID
        {
            get => _CreatedByUserID;
            set => _CreatedByUserID = value;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private string _ReferralUser;

        public string ReferralUser
        {
            get => _ReferralUser.TrimIfNotNull();
            set => _ReferralUser = value.TrimIfNotNull();
        }

        private string _ReferredUser;

        public string ReferredUser
        {
            get => _ReferredUser.TrimIfNotNull();
            set => _ReferredUser = value.TrimIfNotNull();
        }

        private string _CreatedDOC;

        public string CreatedDOC
        {
            get => _CreatedDOC.TrimIfNotNull();
            set => _CreatedDOC = value.TrimIfNotNull();
        }

        private string _BenefitUtilizedDOC;

        public string BenefitUtilizedDOC
        {
            get => _BenefitUtilizedDOC.TrimIfNotNull();
            set => _BenefitUtilizedDOC = value.TrimIfNotNull();
        }

        private decimal _ReferralAmount;

        public decimal ReferralAmount
        {
            get => _ReferralAmount;
            set => _ReferralAmount = value;
        }

        private decimal _TransactionAmount;

        public decimal TransactionAmount
        {
            get => _TransactionAmount;
            set => _TransactionAmount = value;
        }


        private string _UserType;

        public string UserType
        {
            get => _UserType.TrimIfNotNull();
            set => _UserType = value.TrimIfNotNull();
        }
         

        private bool _IsBenefitUtilize;

        public bool IsBenefitUtilize
        {
            get => _IsBenefitUtilize;
            set => _IsBenefitUtilize = value;
        }

        public string IsBenefitUtilizeStr
        {
            get => (IsBenefitUtilize) ? "SUCCESS" : "PROCESS";
        }
    }
}