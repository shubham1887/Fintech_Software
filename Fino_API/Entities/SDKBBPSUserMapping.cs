namespace FintechApp.Entities
{
    public class SDKBBPSUserMapping : BaseEntity
    {
        public int SDKBBPSUserMappingID { get; set; }
        public int UserID { get; set; }
        public string ConfirmationCallbackURL { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public UserMaster UserMaster { get; set; } = null!;
    }
}
