namespace FintechApp.Entities
{
    public class UserwiseServiceChargeMaster : BaseEntity
    {
        public int UserwiseServiceChargeMasterID { get; set; }
        public int ServiceID { get; set; }
        public int UserID { get; set; }
        public decimal OneTime { get; set; }
        public bool IsDelete { get; set; }
        public ServiceMaster ServiceMaster { get; set; } = null!;
        public UserMaster UserMaster { get; set; } = null!;
    }
}
