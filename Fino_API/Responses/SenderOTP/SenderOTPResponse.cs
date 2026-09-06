namespace FintechApp.Responses.SenderOTP {
    public class SenderOTPResponse {
        public int SenderOTPID { get; set; }
        public string Mobile { get; set; } = string.Empty;
        public string OTP { get; set; } = string.Empty;
        public int SenderID { get; set; }
        public int UserID { get; set; }
        public bool IsOTPVerified { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
