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
        /// <summary>
        /// When the amount exceeds the wallet's FREE balance but not its total, release just enough
        /// of the hold (balance cap) to cover the shortfall and reverse the full amount. Off by
        /// default, so a reversal never quietly undoes a hold the admin placed on purpose.
        /// </summary>
        public bool ReleaseHold { get; set; }
    }
}
