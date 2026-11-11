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

        /// <summary>
        /// The logins this mobile number carries, when it carries more than one.
        ///
        /// Empty for the overwhelming majority of accounts, and the client treats empty exactly
        /// as it always did: sign in and go. Populated only when the person has set up a second
        /// role (see StaticModels/LinkedProfilePolicy), and then the client asks which one to
        /// continue as. The token issued alongside this response is always the PRIMARY's; picking
        /// the other one re-issues through Auth/SwitchProfile.
        /// </summary>
        public List<LoginProfileOption> Profiles { get; set; } = new();
    }

    /// <summary>One of the roles a person may continue as after signing in.</summary>
    public class LoginProfileOption
    {
        public int UserID { get; set; }
        public int UserTypeID { get; set; }
        public string UserType { get; set; } = string.Empty;
        public string FirmName { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
    }
}
