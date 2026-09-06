namespace FintechApp.Requests.Fund
{
    public class FundReverseInsertRequest
    {
        public int SenderID { get; set; }
        public int WalletID { get; set; }
        public decimal Amount { get; set; }
        public string Remark { get; set; } = string.Empty;
    }
}
