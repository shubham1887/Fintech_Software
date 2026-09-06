namespace FintechApp.Entities
{
    public class UserPlanUtilization : BaseEntity
    {
        public int UserPlanUtilizationID { get; set; }
        public int UserID { get; set; }
        public int CircleFetchCount { get; set; }
        public int PlanFetchCount { get; set; }
        public int BillFetchCount { get; set; }
        public int TotalPlanCount { get; set; }
        public int TotalCircleCount { get; set; }
        public int TotalBillFetchCount { get; set; }
    }
}
