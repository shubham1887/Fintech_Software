namespace FintechApp.Entities
{
    public class OperationalHistory : BaseEntity
    {
        public int OperationalHistoryID { get; set; }
        public int UserID { get; set; }
        public string APIType { get; set; } = string.Empty;
        public string DocNo { get; set; } = string.Empty;
        public UserMaster UserMaster { get; set; } = null!;
    }
}
