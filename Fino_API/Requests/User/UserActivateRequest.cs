namespace FintechApp.Requests.User
{
    public class UserActivateRequest
    {
        public int TargetUserID { get; set; }
        public bool IsActive { get; set; }
    }

    public class UserCommissionHoldRequest
    {
        public int TargetUserID { get; set; }
        public bool IsCommissionHold { get; set; }
    }

    /// <summary>Soft delete / restore a user. Admin-only (see UsersController).</summary>
    public class UserDeleteRequest
    {
        public int TargetUserID { get; set; }
    }

    public class UserBooleanToggleRequest
    {
        public int TargetUserID { get; set; }
        public bool IsActive { get; set; }
    }
}
