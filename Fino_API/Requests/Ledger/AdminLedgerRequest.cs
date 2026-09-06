namespace FintechApp.Requests.Ledger
{
    public class AdminLedgerRequest
    {
        public string FromDate { get; set; } = string.Empty;
        public string ToDate { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int WalletID { get; set; }
        public bool IsExcelDownload { get; set; }
    }
}
