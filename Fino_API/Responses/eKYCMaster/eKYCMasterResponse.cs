namespace FintechApp.Responses.eKYCMaster {
    public class eKYCMasterResponse {
        public int eKYCMasterID { get; set; }
        public int ProviderID { get; set; }
        public int UserID { get; set; }
        public string OrderID { get; set; } = string.Empty;
        public string OTPGenerateHash { get; set; } = string.Empty;
        public decimal ChargeAmount { get; set; }
        public string OTPGenerateRequest { get; set; } = string.Empty;
        public string OTPGenerateResponse { get; set; } = string.Empty;
        public string VerifyRequest { get; set; } = string.Empty;
        public string VerifyResponse { get; set; } = string.Empty;
        public string OTPToken { get; set; } = string.Empty;
        public string KYCToken { get; set; } = string.Empty;
        public string wadh { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
