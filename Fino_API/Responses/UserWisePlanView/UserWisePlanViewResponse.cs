namespace FintechApp.Responses.UserWisePlanView {
    public class UserWisePlanViewResponse {
        public int UserWisePlanViewID { get; set; }
        public int UserID { get; set; }
        public int ServiceID { get; set; }
        public int OperatorID { get; set; }
        public string ConsumerNumber { get; set; } = string.Empty;
        public int ProviderID { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
