namespace FintechApp.Requests.Ledger
{
    public class SelectLedgerRequest
    {
        public int UserID { get; set; }
        public int UserTypeID { get; set; }
        public string FromDate { get; set; } = string.Empty;
        public string ToDate { get; set; } = string.Empty;
        public int WalletID { get; set; }
        public bool IsExcelDownload { get; set; }
        public bool IsPDFDownload { get; set; }
        public string ExportType { get; set; } = string.Empty;
        public int TakeCount { get; set; }
        public int SkipCount { get; set; }
        public string Search { get; set; } = string.Empty;
        public string SearchText { get; set; } = string.Empty;
        public string Criteria { get; set; } = string.Empty;
        public string CriteriaText { get; set; } = string.Empty;
    }
}
