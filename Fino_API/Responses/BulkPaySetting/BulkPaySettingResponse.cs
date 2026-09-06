namespace FintechApp.Responses.BulkPaySetting {
    public class BulkPaySettingResponse {
        public int BulkPaySettingID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string MerchantID { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Slat { get; set; } = string.Empty;
        public string ActionURL { get; set; } = string.Empty;
        public string StatusCheckURL { get; set; } = string.Empty;
        public bool IsActiveWeb { get; set; }
        public bool IsActive { get; set; }
        public decimal MinimumAmount { get; set; }
        public decimal MaximumAmount { get; set; }
        public string Mode { get; set; } = string.Empty;
    }
}
