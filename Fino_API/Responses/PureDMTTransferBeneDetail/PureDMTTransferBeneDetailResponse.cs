namespace FintechApp.Responses.PureDMTTransferBeneDetail {
    public class PureDMTTransferBeneDetailResponse {
        public int PureDMTTransferBeneDetailID { get; set; }
        public string SenderNumber { get; set; } = string.Empty;
        public int ProviderID { get; set; }
        public string BeneID { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string VerifyBeneName { get; set; } = string.Empty;
        public bool IsVerify { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
