namespace FintechApp.Responses.UserBalanceCapping
{
    public class UserBalanceCappingResponse
    {
        public int UserBalanceCappingID { get; set; }
        public int UserID { get; set; }
        public int WalletID { get; set; }
        public decimal Amount { get; set; }
        public bool IsDelete { get; set; }
        public DateTime DateTime { get; set; }

        public DateTime UpdatedateTime { get; set; }

    }
}

