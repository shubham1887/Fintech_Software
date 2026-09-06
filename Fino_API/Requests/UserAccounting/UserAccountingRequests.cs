namespace FintechApp.Requests.UserAccounting
{
    // Per-user accounting reconciliation for a single date + wallet
    public class UserwiseAccountingRequest
    {
        public string Date { get; set; } = string.Empty;
        /// <summary>
        /// The shared report screen posts a From/To range rather than a single Date. Accounting
        /// reconciles one day at a time, so ToDate is honoured as that day when Date is absent.
        /// </summary>
        public string ToDate { get; set; } = string.Empty;
        public int WalletID { get; set; }
        public int UserID { get; set; }
        public int SkipCount { get; set; }
        public int TakeCount { get; set; } = 50;
        public bool IsExcelDownload { get; set; }

        /// <summary>The day to reconcile: explicit Date, else ToDate, else today.</summary>
        public DateTime EffectiveDate =>
            DateTime.TryParse(Date, out var d) ? d.Date
            : DateTime.TryParse(ToDate, out var t) ? t.Date
            : DateTime.Now.Date;
    }

    // Used by Old/New amount adjustment and manual success-fail list reports
    public class UserAccountingCompRequest
    {
        public string Date { get; set; } = string.Empty;
        public int WalletID { get; set; }
        public int UserID { get; set; }
        public bool IsExcelDownload { get; set; }
    }
}
