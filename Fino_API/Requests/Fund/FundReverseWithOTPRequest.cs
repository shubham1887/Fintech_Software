namespace FintechApp.Requests.Fund
{
    public class FundReverseWithOTPRequest
    {
        public int SenderID { get; set; }
        public int WalletID { get; set; }
        public decimal Amount { get; set; }
        public string Remark { get; set; } = string.Empty;
        public string OTP { get; set; } = string.Empty;
        public string Pin { get; set; } = string.Empty;
    }
}
