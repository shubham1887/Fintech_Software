namespace FintechApp.Responses.CashCollection
{
    public class OutstandingViewModel
    {
        public int UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public decimal Outstanding { get; set; }
    }
}
