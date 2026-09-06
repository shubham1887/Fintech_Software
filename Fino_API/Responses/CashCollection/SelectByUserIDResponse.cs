namespace FintechApp.Responses.CashCollection
{
    public class SelectByUserIDResponse
    {
        public string BalanceDetail { get; set; } = string.Empty;
        public decimal Outstanding { get; set; }
        public decimal MaxOutStanding { get; set; }
    }
}
