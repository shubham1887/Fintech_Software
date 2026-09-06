namespace FintechApp.Responses.BeneficiaryOTP {
    public class BeneficiaryOTPResponse {
        public int BeneficiaryOTPID { get; set; }
        public string SenderMobile { get; set; } = string.Empty;
        public string OTP { get; set; } = string.Empty;
        public int SenderID { get; set; }
        public int BeneficiaryID { get; set; }
        public int UserID { get; set; }
        public bool IsOTPVerified { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
