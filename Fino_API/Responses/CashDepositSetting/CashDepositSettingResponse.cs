namespace FintechApp.Responses.CashDepositSetting
{
    public class CashDepositSettingResponse
    {
        public int CashDepositSettingID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string MerchantID { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Slat { get; set; } = string.Empty;
        public string? PublicKey { get; set; }
        public bool IsActiveWeb { get; set; }
        public bool IsActive { get; set; }
        public decimal MinimumAmount { get; set; }
        public decimal MaximumAmount { get; set; }
        public string? Mode { get; set; }
    }
}
