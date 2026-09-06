namespace FintechApp.Responses.UserwiseLowBalance
{
    public class UserwiseLowBalanceResponse
    {
        public int UserwiseLowBalanceID { get; set; }
        public int UserID { get; set; }
        public int WalletID { get; set; }
        public decimal Amount { get; set; }
    }
}
