namespace FintechApp.Requests.Login
{
    public class ForgotPasswordGenerateOtpRequest
    {
        public string MobileNumber { get; set; } = string.Empty;
    }

    public class ForgotPasswordVerifyOtpRequest
    {
        public string MobileNumber { get; set; } = string.Empty;
        public string OTP { get; set; } = string.Empty;
    }
}
