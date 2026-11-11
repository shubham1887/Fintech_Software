namespace FintechApp.Responses.User
{
    /// <summary>One login of a person who holds two roles on one mobile number.</summary>
    public class LinkedProfileItem
    {
        public int UserID { get; set; }
        public int UserTypeID { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string FirmName { get; set; } = string.Empty;
        public string PersonName { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        /// <summary>The account that owns the pair — the one sign-in authenticates against.</summary>
        public bool IsPrimary { get; set; }
        /// <summary>The one this session is currently working as.</summary>
        public bool IsCurrent { get; set; }
        public bool IsActive { get; set; }
    }

    public class LinkedProfileRoleOption
    {
        public int UserTypeID { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Everything the profile screen and the account switcher need: the logins this person has,
    /// whether a second one may be added, and — when it may not — why not, so the screen can say
    /// so rather than render an empty card.
    /// </summary>
    public class LinkedProfilesResponse
    {
        public int PrimaryUserID { get; set; }
        public int CurrentUserID { get; set; }
        public List<LinkedProfileItem> Profiles { get; set; } = new();
        public bool CanSetUp { get; set; }
        public List<LinkedProfileRoleOption> AllowedRoles { get; set; } = new();
        public string Reason { get; set; } = string.Empty;
        /// <summary>More than one login — the only condition under which the switcher is shown.</summary>
        public bool HasMultiple => Profiles.Count > 1;
    }
}
