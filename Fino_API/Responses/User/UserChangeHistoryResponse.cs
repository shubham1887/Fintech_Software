namespace FintechApp.Responses.User
{
    /// <summary>
    /// One row of the user change report: what changed on whose account, who changed it, why, and
    /// from where.
    /// </summary>
    public class UserChangeHistoryViewModel
    {
        public int UserChangeHistoryID { get; set; }
        public DateTime DOC { get; set; }

        public int UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserMobile { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;

        /// <summary>CREATE / UPDATE / ACTIVATE / DEACTIVATE / DELETE / TRANSFER / …</summary>
        public string Action { get; set; } = string.Empty;

        public string FieldName { get; set; } = string.Empty;
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;

        /// <summary>Why. The column this report exists for.</summary>
        public string Reason { get; set; } = string.Empty;

        public int ChangedByUserID { get; set; }
        public string ChangedByName { get; set; } = string.Empty;
        public string ChangedByType { get; set; } = string.Empty;

        public string IPAddress { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
    }

    public class UserChangeHistoryWrapper
    {
        public int Count { get; set; }
        public List<UserChangeHistoryViewModel> list { get; set; } = new();
    }
}
