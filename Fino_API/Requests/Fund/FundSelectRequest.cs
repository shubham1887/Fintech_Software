namespace FintechApp.Requests.Fund
{
    public class FundSelectRequest
    {
        public string FromDate { get; set; } = string.Empty;
        public string ToDate { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string DOCType { get; set; } = string.Empty;
        public int SkipCount { get; set; }
        public int TakeCount { get; set; } = 20;
        public string Search { get; set; } = string.Empty;
        public int UserID { get; set; }
        public int WalletID { get; set; }
        public string ReportFor { get; set; } = string.Empty;
        public string PayMode { get; set; } = string.Empty;
    }
}
