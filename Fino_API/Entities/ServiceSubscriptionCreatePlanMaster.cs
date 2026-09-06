namespace FintechApp.Entities
{
    public class ServiceSubscriptionCreatePlanMaster : BaseEntity
    {
        public int ServiceSubscriptionCreatePlanMasterID { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string ImageURL { get; set; } = string.Empty;
        public bool IsDefault { get; set; }

        /// <summary>Validity in days; the subscription expires this many days after activation. 0 = lifetime.</summary>
        public int ValidityDays { get; set; }
    }
}
