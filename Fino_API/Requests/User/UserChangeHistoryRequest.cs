namespace FintechApp.Requests.User
{
    /// <summary>Filter for the user change-history report (Users/ChangeHistory).</summary>
    public class UserChangeHistoryRequest
    {
        public string FromDate { get; set; } = string.Empty;
        public string ToDate { get; set; } = string.Empty;

        /// <summary>The user whose record was changed. 0 = everyone the caller may see.</summary>
        public int UserID { get; set; }

        /// <summary>Narrow to one operator — "everything this admin changed". 0 = anyone.</summary>
        public int ChangedByUserID { get; set; }

        /// <summary>CREATE / UPDATE / ACTIVATE / … . Empty or "ALL" = no filter.</summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>Free text over user, operator, field, values and reason.</summary>
        public string Search { get; set; } = string.Empty;

        public int SkipCount { get; set; }
        public int TakeCount { get; set; } = 200;
    }

    /// <summary>
    /// Move one user (and, implicitly, their whole branch) under a new parent — Users/TransferUsers.
    /// </summary>
    public class UserTransferRequest
    {
        /// <summary>The user being moved.</summary>
        public int UserID { get; set; }

        /// <summary>The user they are moved under. 0 is rejected; use the admin's own id to attach to admin.</summary>
        public int NewParentID { get; set; }

        /// <summary>Why. Required — a silent re-parent is unexplainable afterwards.</summary>
        public string Remark { get; set; } = string.Empty;
    }
}
