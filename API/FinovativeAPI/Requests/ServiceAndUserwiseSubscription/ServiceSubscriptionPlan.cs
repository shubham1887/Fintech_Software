using Project.Processing;

namespace Project.Requests.ServiceAndUserwiseSubscription
{
    public class ServiceSubscriptionPlan
    {
  
        private int _ServiceSubscriptionPlanMasterID;
        public int ServiceSubscriptionPlanMasterID
        {
            get => _ServiceSubscriptionPlanMasterID;
            set => _ServiceSubscriptionPlanMasterID = value;
        }
        private string _PlanName;
        public string PlanName
        {
            get => _PlanName.TrimIfNotNull();
            set => _PlanName = value.TrimIfNotNull();
        }
    }
}
