namespace FintechApp.Responses.User
{
    public class UserResponse
    {
        public int UserMasterID { get; set; }
        public string PersonName { get; set; } = string.Empty;
        public string FirmName { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string EmailID { get; set; } = string.Empty;
        public int UserTypeID { get; set; }
        public string UserType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsKYCApproved { get; set; }
        public string StateName { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
    }
}
