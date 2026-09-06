namespace FintechApp.Requests.DailyAuthentication
{
    public class SelectDailyAuthRequest
    {
        public string ProviderName { get; set; } = string.Empty;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
