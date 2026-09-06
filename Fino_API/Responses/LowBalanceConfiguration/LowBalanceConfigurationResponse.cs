namespace FintechApp.Responses.LowBalanceConfiguration
{
    public class LowBalanceConfigurationResponse
    {
        public int LowBalanceConfigurationID { get; set; }
        public int UserID { get; set; }
        public int WalletID { get; set; }
        public decimal Amount { get; set; }
        public string? MobileNumber { get; set; }
        public string? EmailID { get; set; }
        public string? WebHookURL { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateTime { get; set; }
    }
}
