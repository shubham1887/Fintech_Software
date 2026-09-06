namespace FintechApp.Entities
{
    public class SDKSetting
    {
        public int SDKSettingID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string MerchantID { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string URL { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
