namespace FintechApp.Entities
{
    public class ServiceSubscriptionPlanDetails : BaseEntity
    {
        public int ServiceSubscriptionPlanDetailsID { get; set; }
        public int ServiceSubscriptionCreatePlanMasterID { get; set; }
        public int ServiceID { get; set; }
        public ServiceSubscriptionCreatePlanMaster ServiceSubscriptionCreatePlanMaster { get; set; } = null!;
    }
}
