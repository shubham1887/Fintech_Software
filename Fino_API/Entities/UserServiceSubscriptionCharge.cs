namespace FintechApp.Entities
{
    public class UserServiceSubscriptionCharge : BaseEntity
    {
        public int UserServiceSubscriptionChargeID { get; set; }
        public int UserID { get; set; }
        public int ServiceID { get; set; }
        public int ServiceAndUserwiseSubscriptionID { get; set; }
        public decimal Charge { get; set; }
        public int Sequence { get; set; }
        public bool IsAdminApproved { get; set; }
    }
}
