namespace FintechApp.Requests.Ledger
{
    public class AdminLedgerRequest
    {
        public string FromDate { get; set; } = string.Empty;
        public string ToDate { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int WalletID { get; set; }
        public bool IsExcelDownload { get; set; }
        /// <summary>Rows per page, as the report screen sends it. 0 = everything (a download).</summary>
        public int TakeCount { get; set; }
        /// <summary>Rows already shown. Without these two this ledger answered with the whole
        /// period on every page, so the pager had nothing to page.</summary>
        public int SkipCount { get; set; }
    }
}
