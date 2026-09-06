using Project.Processing;

namespace Project.Response.ServiceSubscription
{
    public class AssignPlanViewModel
    {
        private int _AssignSubscriptionPlanID;
        public int AssignSubscriptionPlanID
        {
            get => _AssignSubscriptionPlanID;
            set => _AssignSubscriptionPlanID = value;
        }

        private int _ServiceSubscriptionCreatePlanMasterID;
        public int ServiceSubscriptionCreatePlanMasterID
        {
            get => _ServiceSubscriptionCreatePlanMasterID;
            set => _ServiceSubscriptionCreatePlanMasterID = value;
        }

        private string _PlanName;

        public string PlanName
        {
            get => _PlanName.TrimIfNotNull();
            set => _PlanName = value.TrimIfNotNull();
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private decimal _TotalAmount;

        public decimal TotalAmount
        {
            get => _TotalAmount;
            set => _TotalAmount = value;
        }

        private decimal _GST;

        public decimal GST
        {
            get => _GST;
            set => _GST = value;
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
    }
}