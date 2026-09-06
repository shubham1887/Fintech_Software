namespace FintechApp.Responses.ReferralRegistration {
    public class ReferralRegistrationResponse {
        public int ReferralRegistrationID { get; set; }
        public int UserID { get; set; }
        public string MobileNumber { get; set; } = string.Empty;
        public string ReferralCode { get; set; } = string.Empty;
        public int CreatedByUserID { get; set; }
        public bool IsBenefitUtilize { get; set; }
        public decimal ReferralAmount { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
