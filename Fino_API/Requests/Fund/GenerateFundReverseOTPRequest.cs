namespace FintechApp.Requests.Fund
{
    public class GenerateFundReverseOTPRequest
    {
        public int SenderID { get; set; }
        public decimal Amount { get; set; }
        public int WalletID { get; set; }
        public string Pin { get; set; } = string.Empty;
        public string Remark { get; set; } = string.Empty;
    }
}
