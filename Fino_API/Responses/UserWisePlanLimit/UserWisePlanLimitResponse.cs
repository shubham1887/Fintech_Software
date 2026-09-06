namespace FintechApp.Responses.UserWisePlanLimit {
    public class UserWisePlanLimitResponse {
        public int UserWisePlanLimitID { get; set; }
        public int UserID { get; set; }
        public int UserTypeID { get; set; }
        public int CircleHitCount { get; set; }
        public int PlanHitCount { get; set; }
        public int BillFetchHitCount { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
