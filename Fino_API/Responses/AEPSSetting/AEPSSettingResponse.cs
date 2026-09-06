namespace FintechApp.Responses.AEPSSetting
{
    public class AEPSSettingResponse
    {
        public int AEPSSettingID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string MerchantID { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Slat { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
        public bool IsActiveWeb { get; set; }
        public bool IsActive { get; set; }
        public decimal MinimumAmount { get; set; }
        public decimal MaximumAmount { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public int Sequence { get; set; }
        public string Mode { get; set; } = string.Empty;
        public bool IsMerchantVerify { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
