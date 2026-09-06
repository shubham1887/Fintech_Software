namespace FintechApp.Entities
{
    public class BeneficiaryOTP : BaseEntity
    {
        public int BeneficiaryOTPID { get; set; }
        public string SenderMobile { get; set; } = string.Empty;
        public string OTP { get; set; } = string.Empty;
        public int SenderID { get; set; }
        public int BeneficiaryID { get; set; }
        public int UserID { get; set; }
        public bool IsOTPVerified { get; set; }
        public Sender Sender { get; set; } = null!;
        public Beneficiary Beneficiary { get; set; } = null!;
        public UserMaster UserMaster { get; set; } = null!;
    }
}
