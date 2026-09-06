namespace FintechApp.Entities
{
    public class AuthorizationMaster : BaseEntity
    {
        public int AuthorizationMasterID { get; set; }
        public string Controller { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public int UserTypeID { get; set; }
        public bool IsActive { get; set; }
        public UserTypeMaster UserTypeMaster { get; set; } = null!;
    }
}
