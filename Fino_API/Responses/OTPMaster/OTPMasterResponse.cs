namespace FintechApp.Responses.OTPMaster
{
    public class OTPMasterResponse
    {
        public int OTPMasterID { get; set; }
        public string? OTP { get; set; }
        public bool IsVerified { get; set; }
        public string? Type { get; set; }
        public int UserID { get; set; }
        public string? MobileNumber { get; set; }
        public DateTime DateTime { get; set; }
    }
}
