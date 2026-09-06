namespace FintechApp.Entities
{
    public class UserServiceSubscriptionDetails : BaseEntity
    {
        public int UserServiceSubscriptionDetailsID { get; set; }
        public int UserID { get; set; }
        public int ServiceID { get; set; }
        public int ServiceAndUserwiseSubscriptionID { get; set; }
        public decimal Charge { get; set; }
        public UserMaster UserMaster { get; set; } = null!;
        public ServiceMaster ServiceMaster { get; set; } = null!;
    }
}
