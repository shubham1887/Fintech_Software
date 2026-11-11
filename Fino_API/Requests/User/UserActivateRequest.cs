namespace FintechApp.Requests.User
{
    public class UserActivateRequest
    {
        public int TargetUserID { get; set; }
        public bool IsActive { get; set; }

        /// <summary>
        /// Why. Recorded on the user change history, which is the report that answers "who
        /// switched this retailer off, and on whose instruction". Optional on the wire so an
        /// older client still works; the panel asks for it.
        /// </summary>
        public string Reason { get; set; } = string.Empty;
    }

    public class UserCommissionHoldRequest
    {
        public int TargetUserID { get; set; }
        public bool IsCommissionHold { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>Soft delete / restore a user. Admin-only (see UsersController).</summary>
    public class UserDeleteRequest
    {
        public int TargetUserID { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class UserBooleanToggleRequest
    {
        public int TargetUserID { get; set; }
        public bool IsActive { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
