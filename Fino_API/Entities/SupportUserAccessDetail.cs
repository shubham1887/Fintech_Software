namespace FintechApp.Entities
{
    public class SupportUserAccessDetail : BaseEntity
    {
        public int SupportUserAccessDetailID { get; set; }
        public int UserID { get; set; }
        public string Controller { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string MenuGUID { get; set; } = string.Empty;
        public UserMaster UserMaster { get; set; } = null!;
    }
}
