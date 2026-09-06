namespace FintechApp.Responses.UserMerchantMaster {
    public class UserMerchantMasterResponse {
        public int UserMerchantMasterID { get; set; }
        public int UserID { get; set; }
        public string PaySprintUPIMerchantCode { get; set; } = string.Empty;
        public string PaySprintStaticQR { get; set; } = string.Empty;
        public string PaySprintUPIRefid { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
