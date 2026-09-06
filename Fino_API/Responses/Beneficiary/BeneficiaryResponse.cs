namespace FintechApp.Responses.Beneficiary {
    public class BeneficiaryResponse {
        public int BeneficiaryID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string AccountNo { get; set; } = string.Empty;
        public string IFSC { get; set; } = string.Empty;
        public string GUID { get; set; } = string.Empty;
        public string SenderMobile { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
        public int SenderID { get; set; }
        public int UserID { get; set; }
        public bool IsActive { get; set; }
        public bool IsOTPVerified { get; set; }
        public bool IsNameVerified { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
