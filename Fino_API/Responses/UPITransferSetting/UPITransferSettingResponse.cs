namespace FintechApp.Responses.UPITransferSetting {
    public class UPITransferSettingResponse {
        public int UPITransferSettingID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string CallBackURL { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public decimal SenderLimit { get; set; }
        public decimal MaximumAmount { get; set; }
        public decimal MinAmount { get; set; }
        public int Sequence { get; set; }
        public string AuthCode { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
