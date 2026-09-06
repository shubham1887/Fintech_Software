namespace FintechApp.Responses.Login
{
    public class LoginResponse
    {
        public int UserID { get; set; }
        public string PersonName { get; set; } = string.Empty;
        public string FirmName { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string EmailID { get; set; } = string.Empty;
        public int UserTypeID { get; set; }
        public string UserType { get; set; } = string.Empty;
        public string SessionToken { get; set; } = string.Empty;
        public DateTime SessionExpiry { get; set; }
        public bool IsKYCApproved { get; set; }
        public bool IsTwoFA { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public bool IsInitialPassword { get; set; }
        public bool IsPasswordExpired { get; set; }
        public bool IsLoginOTP { get; set; }
        public bool IsNewDevice { get; set; }
        public bool IsOTPSent { get; set; }
        public string LoginIP { get; set; } = string.Empty;
        public int PasswordExpiryDays { get; set; }
        public string ProfileImagePath { get; set; } = string.Empty;
        public string WhatsAppNumber { get; set; } = string.Empty;
    }
}
