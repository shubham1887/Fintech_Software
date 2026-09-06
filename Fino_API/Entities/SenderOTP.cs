namespace FintechApp.Entities
{
    public class SenderOTP : BaseEntity
    {
        public int SenderOTPID { get; set; }
        public string Mobile { get; set; } = string.Empty;
        public string OTP { get; set; } = string.Empty;
        public int SenderID { get; set; }
        public int UserID { get; set; }
        public bool IsOTPVerified { get; set; }
        public Sender Sender { get; set; } = null!;
        public UserMaster UserMaster { get; set; } = null!;
    }
}
