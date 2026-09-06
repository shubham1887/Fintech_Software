namespace FintechApp.Responses.OnboardingUser {
    public class OnboardingUserResponse {
        public int OnboardingUserID { get; set; }
        public string ExternalID { get; set; } = string.Empty;
        public string ProfileID { get; set; } = string.Empty;
        public string ReferenceID { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DOB { get; set; } = string.Empty;
        public string PanNo { get; set; } = string.Empty;
        public string AadhaarNo { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string EmailID { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PinCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string AdminStatus { get; set; } = string.Empty;
        public string SupportStatus { get; set; } = string.Empty;
        public string AdminRemark { get; set; } = string.Empty;
        public string SupportRemark { get; set; } = string.Empty;
        public string OrderID { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string IFSC { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public int SlabID { get; set; }
        public string AgentID { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
