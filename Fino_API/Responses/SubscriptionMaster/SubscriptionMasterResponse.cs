namespace FintechApp.Responses.SubscriptionMaster {
    public class SubscriptionMasterResponse {
        public int SubscriptionMasterID { get; set; }
        public DateTime StartDate { get; set; } = DateTime.MinValue;
        public string EndDate { get; set; } = string.Empty;
        public DateTime UpdatedDOC { get; set; } = DateTime.MinValue;
        public int UpdatedDOCInt { get; set; }
        public bool IsActive { get; set; }
    }
}
