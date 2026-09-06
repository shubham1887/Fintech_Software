namespace FintechApp.Requests.Credential
{
    public class LoginCredentialRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Device { get; set; } = string.Empty;
        public string DeviceID { get; set; } = string.Empty;
        public string FingerPrint { get; set; } = string.Empty;
        public string AppVersion { get; set; } = string.Empty;
        public int Pin { get; set; }
    }

    public class VerifyTwoFARequest
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Device { get; set; } = string.Empty;
        public string DeviceID { get; set; } = string.Empty;
        public string FingerPrint { get; set; } = string.Empty;
        public string TwoFAPasscode { get; set; } = string.Empty;
    }

    public class ForgotPasswordCredentialRequest
    {
        public string MobileNumber { get; set; } = string.Empty;
        public string PANNo { get; set; } = string.Empty;
    }

    public class ForgotPasswordOTPRequest
    {
        public string MobileNumber { get; set; } = string.Empty;
        public string OTP { get; set; } = string.Empty;
    }

    public class NewDeviceLoginRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Device { get; set; } = string.Empty;
        public string DeviceID { get; set; } = string.Empty;
        public string FingerPrint { get; set; } = string.Empty;
        public string OTP { get; set; } = string.Empty;
    }

    public class UpdatePasswordCredentialRequest
    {
        public string MobileNumber { get; set; } = string.Empty;
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ChangePasswordAfterExpiryRequest
    {
        public string MobileNumber { get; set; } = string.Empty;
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
