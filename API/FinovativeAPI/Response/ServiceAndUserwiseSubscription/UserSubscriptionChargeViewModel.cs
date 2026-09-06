using Project.Processing;

namespace Project.Response.ServiceAndUserwiseSubscription
{
    public class UserSubscriptionChargeViewModel
    {
        private int _ServiceAndUserwiseSubscriptionID;

        public int ServiceAndUserwiseSubscriptionID
        {
            get => _ServiceAndUserwiseSubscriptionID;
            set => _ServiceAndUserwiseSubscriptionID = value;
        }


        private string _StartDate;

        public string StartDate
        {
            get => _StartDate.TrimIfNotNull();
            set => _StartDate = value.TrimIfNotNull();
        }

        private string _EndDate;

        public string EndDate
        {
            get => _EndDate.TrimIfNotNull();
            set => _EndDate = value.TrimIfNotNull();
        }

        private string _PaymentEndDate;

        public string PaymentEndDate
        {
            get => _PaymentEndDate.TrimIfNotNull();
            set => _PaymentEndDate = value.TrimIfNotNull();
        }


        private string _ServiceName;

        public string ServiceName
        {
            get => _ServiceName.TrimIfNotNull();
            set => _ServiceName = value.TrimIfNotNull();
        }

        private string _SubscriptionType;

        public string SubscriptionType
        {
            get => _SubscriptionType.TrimIfNotNull();
            set => _SubscriptionType = value.TrimIfNotNull();
        }

        private decimal _Charge;
        public decimal Charge { get => _Charge; set => _Charge = value; }

        private int _DueDays;

        public int DueDays
        {
            get => _DueDays;
            set => _DueDays = value;
        }

        public bool IsPaymentOverDue { get; internal set; }


        private string _PlanName;

        public string PlanName
        {
            get => _PlanName.TrimIfNotNull();
            set => _PlanName = value.TrimIfNotNull();
        }

        private decimal _OriginalCharge;
        public decimal OriginalCharge
        {
            get => _OriginalCharge;
            set => _OriginalCharge = value;
        }

        private decimal _Discount;
        public decimal Discount
        {
            get => _Discount;
            set => _Discount = value;
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _UserType;

        public string UserType
        {
            get => _UserType.TrimIfNotNull();
            set => _UserType = value.TrimIfNotNull();
        }

        private string _UserName;

        public string UserName
        {
            get => _UserName.TrimIfNotNull();
            set => _UserName = value.TrimIfNotNull();
        }
        private string _LoginID;

        public string LoginID
        {
            get => _LoginID.TrimIfNotNull();
            set => _LoginID = value.TrimIfNotNull();
        }

    }
}
