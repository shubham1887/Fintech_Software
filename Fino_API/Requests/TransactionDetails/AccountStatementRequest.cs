namespace FintechApp.Requests.TransactionDetails
{
    /// <summary>
    /// Filter for Account Statement / Admin Accounting (TransactionDetails/SelectForFilter,
    /// SelectAdminAccounting). Mirrors the fields the web report screen sends.
    /// </summary>
    public class AccountStatementRequest
    {
        public int UserID { get; set; }
        public string FromDate { get; set; } = string.Empty;
        public string ToDate { get; set; } = string.Empty;
        /// <summary>"" = all, "Credit" or "Debit" to filter one side.</summary>
        public string AmountCreditDebit { get; set; } = string.Empty;

        /// <summary>
        /// 0 = every wallet, or one WalletID to read a single wallet's statement. Without it the
        /// opening and closing balances on a statement belong to whichever wallet each row happened
        /// to move in, so a balance read off a multi-wallet statement is not the balance of
        /// anything.
        /// </summary>
        public int WalletID { get; set; }

        /// <summary>
        /// True = return every matching row rather than one page, for the export. Paging the
        /// export is how a month's statement comes out as its first fifty lines.
        /// </summary>
        public bool IsExcelDownload { get; set; }
        /// <summary>Optional search field key (ConsumerNumber / TransactionID) + its value.</summary>
        public string Criteria { get; set; } = string.Empty;
        public string CriteriaText { get; set; } = string.Empty;
        public int SkipCount { get; set; }
        public int TakeCount { get; set; } = 50;
    }
}
