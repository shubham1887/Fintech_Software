namespace FintechApp.Responses.Ledger
{
    /// <summary>
    /// One wallet-wise accounting row of the company (admin) account over a date range:
    /// opening / closing wallet balance, business Sale &amp; Purchase volume, the admin wallet's
    /// Credit / Debit movement and the commission earned.
    /// </summary>
    public class AdminReconciliationViewModel
    {
        public int WalletID { get; set; }
        public string WalletName { get; set; } = string.Empty;
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public decimal Sale { get; set; }
        public decimal Purchase { get; set; }
        public decimal Credit { get; set; }
        public decimal Debit { get; set; }
        public decimal Commission { get; set; }
    }

    public class AdminReconciliationWrapper
    {
        public List<AdminReconciliationViewModel> data { get; set; } = new();
        /// <summary>Column totals across every returned wallet row.</summary>
        public AdminReconciliationViewModel Totals { get; set; } = new() { WalletName = "Total" };
        public int Count { get; set; }
        public bool IsExcelDownload { get; set; }
    }
}
