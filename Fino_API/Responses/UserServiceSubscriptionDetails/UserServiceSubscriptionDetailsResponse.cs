namespace FintechApp.Responses.UserServiceSubscriptionDetails {
    public class UserServiceSubscriptionDetailsResponse {
        public int UserServiceSubscriptionDetailsID { get; set; }
        public int UserID { get; set; }
        public int ServiceID { get; set; }
        public int ServiceAndUserwiseSubscriptionID { get; set; }
        public decimal Charge { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
