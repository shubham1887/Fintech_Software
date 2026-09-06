namespace FintechApp.Responses.UserBalanceHistory
{
    public class UserBalanceHistoryResponse
    {
        public int UserBalanceHistoryID { get; set; }
        public int UserID { get; set; }
        public int WalletID { get; set; }
        public decimal Balance { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }

    }
}

