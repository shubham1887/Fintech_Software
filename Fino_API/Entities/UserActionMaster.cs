namespace FintechApp.Entities
{
    public class UserActionMaster : BaseEntity
    {
        public int UserActionMasterID { get; set; }
        public int UserID { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public string FingurePrint { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
        public UserMaster UserMaster { get; set; } = null!;
    }
}
