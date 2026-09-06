namespace FintechApp.Responses.Fund
{
    public class GenerateFundReverseOTPResponse
    {
        public bool IsOTPSent { get; set; }
        public decimal Amount { get; set; }
        public int WalletID { get; set; }
        public int SenderID { get; set; }
        public string Pin { get; set; } = string.Empty;
        public string Remark { get; set; } = string.Empty;
    }
}
