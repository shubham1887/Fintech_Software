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
        /// <summary>Optional search field key (ConsumerNumber / TransactionID) + its value.</summary>
        public string Criteria { get; set; } = string.Empty;
        public string CriteriaText { get; set; } = string.Empty;
        public int SkipCount { get; set; }
        public int TakeCount { get; set; } = 50;
    }
}
