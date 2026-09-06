namespace FintechApp.Entities
{
    public class UserWisePlanLimit : BaseEntity
    {
        public int UserWisePlanLimitID { get; set; }
        public int UserID { get; set; }
        public int UserTypeID { get; set; }
        public int CircleHitCount { get; set; }
        public int PlanHitCount { get; set; }
        public int BillFetchHitCount { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public UserMaster UserMaster { get; set; } = null!;
    }
}
