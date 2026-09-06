namespace FintechApp.Entities
{
    public class CMSSetting
    {
        public int CMSSettingID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string MerchantID { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Slat { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
        public bool IsActiveWeb { get; set; }
        public bool IsActive { get; set; }
        public string Mode { get; set; } = string.Empty;
    }
}
