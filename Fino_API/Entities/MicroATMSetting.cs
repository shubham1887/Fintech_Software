namespace FintechApp.Entities
{
    public class MicroATMSetting
    {
        public int MicroATMSettingID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string MerchantID { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Slat { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
        public decimal MinimumAmount { get; set; }
        public decimal MaximumAmount { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public int Sequence { get; set; }
        public bool IsActive { get; set; }
        public bool IsActiveWeb { get; set; }
    }
}
