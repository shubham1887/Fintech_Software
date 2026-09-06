namespace FintechApp.Responses.TransactionReport
{
    /// <summary>
    /// Provider Sale row (TransactionReport/ProviderSale). Purchase / provider opening-closing
    /// balances are not tracked yet and are returned as 0. Property names match the web columns.
    /// </summary>
    public class ProviderSaleViewModel
    {
        public int ID { get; set; }
        public string ProvderName { get; set; } = string.Empty; // legacy spelling kept for the web column key
        public decimal Purchase { get; set; }
        public decimal Sale { get; set; }
        public decimal Fail { get; set; }
        public decimal RetailerCommission { get; set; }
        public decimal DistributerCommission { get; set; }
        public decimal SuperDistributerCommission { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal AdminCommission { get; set; }
        public decimal ClosingBalance { get; set; }
        public decimal Difference { get; set; }
        public decimal Profit { get; set; }
    }

    public class ProviderSaleWrapper
    {
        public int Count { get; set; }
        public List<ProviderSaleViewModel> list { get; set; } = new();
    }

    /// <summary>
    /// Purchase Report row (TransactionReport/PurchaseReport) — how much we paid each provider
    /// (the manual ProviderPurchase entries), aggregated per provider for a period. The period is
    /// a month ("2026-08") or a day ("2026-08-25") depending on the request's GroupBy.
    /// </summary>
    public class PurchaseReportViewModel
    {
        public int ID { get; set; }
        public string Period { get; set; } = string.Empty;
        public string ProviderName { get; set; } = string.Empty;
        public string WalletName { get; set; } = string.Empty;
        public int Payments { get; set; }
        public decimal Amount { get; set; }
    }

    public class PurchaseReportWrapper
    {
        public int Count { get; set; }
        public List<PurchaseReportViewModel> list { get; set; } = new();
    }
}
