namespace FintechApp.Responses.Ledger
{
    public class AdminLedgerViewModel
    {
        public string DOC { get; set; } = string.Empty;
        public string WalletName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal OpeningBalance { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal ClosingBalance { get; set; }
        public string Through { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string Remark { get; set; } = string.Empty;
        public int WalletID { get; set; }
    }

    public class AdminLedgerWrapper
    {
        public List<AdminLedgerViewModel> data { get; set; } = new();
        public string ReportPath { get; set; } = string.Empty;
        public bool IsExcelDownload { get; set; }
        public int Count { get; set; }
    }
}
