namespace FintechApp.Responses.ServiceSubscriptionCreatePlanMaster {
    public class ServiceSubscriptionCreatePlanMasterResponse {
        public int ServiceSubscriptionCreatePlanMasterID { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string ImageURL { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
