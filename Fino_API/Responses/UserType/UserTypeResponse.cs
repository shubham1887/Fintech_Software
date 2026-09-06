namespace FintechApp.Responses.UserType
{
    public class UserTypeResponse
    {
        public int UserTypeID { get; set; }
        public string UserType { get; set; } = string.Empty;
        public string AliasName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime DateTime { get; set; }
    }

    public class UserTypeDropdownResponse
    {
        public int UserTypeID { get; set; }
        public string UserType { get; set; } = string.Empty;
        public string AliasName { get; set; } = string.Empty;
    }
}
