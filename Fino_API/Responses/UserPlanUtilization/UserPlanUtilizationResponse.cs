namespace FintechApp.Responses.UserPlanUtilization {
    public class UserPlanUtilizationResponse {
        public int UserPlanUtilizationID { get; set; }
        public int UserID { get; set; }
        public int CircleFetchCount { get; set; }
        public int PlanFetchCount { get; set; }
        public int BillFetchCount { get; set; }
        public int TotalPlanCount { get; set; }
        public int TotalCircleCount { get; set; }
        public int TotalBillFetchCount { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
