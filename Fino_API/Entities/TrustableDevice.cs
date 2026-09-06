namespace FintechApp.Entities
{
    public class TrustableDevice : BaseEntity
    {
        public int ID { get; set; }
        public string IPAddress { get; set; } = string.Empty;
        public int UserID { get; set; }
        public bool IsVerified { get; set; }
        public string Browser { get; set; } = string.Empty;
        public string Device { get; set; } = string.Empty;
        public UserMaster UserMaster { get; set; } = null!;
    }
}
