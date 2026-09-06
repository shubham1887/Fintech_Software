namespace FintechApp.Requests.eKYC
{
    public class eKYCValidateRequest
    {
        public string Otp { get; set; } = string.Empty;
        public string otpToken { get; set; } = string.Empty;
        public int eKYCMasterID { get; set; }
        public string RequestID { get; set; } = string.Empty;
    }
}
