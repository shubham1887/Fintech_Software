namespace FintechApp.Responses.PanCardSetting {
    public class PanCardSettingResponse {
        public int PanCardSettingID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string MerchantID { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Slat { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
        public string Mode { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
