namespace FintechApp.Responses.eKYCSetting {
    public class eKYCSettingResponse {
        public int eKYCSettingID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slat { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string AuthKey { get; set; } = string.Empty;
        public decimal ChargeAmount { get; set; }
        public string EncryptionKey { get; set; } = string.Empty;
        public string MerchantID { get; set; } = string.Empty;
        public string Mode { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsActiveWeb { get; set; }
    }
}
