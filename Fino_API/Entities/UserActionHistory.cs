namespace FintechApp.Entities
{
    public class UserActionHistory : BaseEntity
    {
        public int UserHistoryID { get; set; }
        public int UserID { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
        public UserMaster UserMaster { get; set; } = null!;
    }
}
