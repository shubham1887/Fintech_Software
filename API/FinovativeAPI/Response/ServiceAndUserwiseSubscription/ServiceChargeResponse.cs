using Project.Processing;
using System;

namespace Project.Response.ServiceAndUserwiseSubscription
{
    public class ServiceChargeResponse
    {
        private int _ServiceAndUserwiseSubscriptionID;

        public int ServiceAndUserwiseSubscriptionID
        {
            get => _ServiceAndUserwiseSubscriptionID;
            set => _ServiceAndUserwiseSubscriptionID = value;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private string _EndDate;

        public string EndDate
        {
            get => _EndDate.TrimIfNotNull();
            set => _EndDate = value.TrimIfNotNull();
        }

        private string _StartDate;

        public string StartDate
        {
            get => _StartDate.TrimIfNotNull();
            set => _StartDate = value.TrimIfNotNull();
        }

        private string _SubscriptionType;

        public string SubscriptionType
        {
            get => _SubscriptionType.TrimIfNotNull();
            set => _SubscriptionType = value.TrimIfNotNull();
        }

        private string _PlanName;

        public string PlanName
        {
            get => _PlanName.TrimIfNotNull();
            set => _PlanName = value.TrimIfNotNull();
        }

        private string _ServiceName;

        public string ServiceName
        {
            get => _ServiceName.TrimIfNotNull();
            set => _ServiceName = value.TrimIfNotNull();
        }

        private decimal _OriginalCharge;
        public decimal OriginalCharge
        {
            get => _OriginalCharge;
            set => _OriginalCharge = value;
        }

        private decimal _Charge;
        public decimal Charge
        {
            get => _Charge;
            set => _Charge = value;
        }
    }
}