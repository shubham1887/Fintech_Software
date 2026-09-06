namespace FintechApp.Requests.Fund
{
    public class FundTransferRequest
    {
        public int ReceiverID { get; set; }
        public int WalletID { get; set; }
        public decimal Amount { get; set; }
        public string Remark { get; set; } = string.Empty;
        public string PayMode { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        // Master-driven method + source/destination (same as Fund Request). Optional.
        public int ModeID { get; set; }
        public int SubModeID { get; set; }
        public string UTRNumber { get; set; } = string.Empty;
        public string TransactionDate { get; set; } = string.Empty;
        public string Document { get; set; } = string.Empty;  // uploaded slip/receipt path
        public decimal CashCollectionAmount { get; set; }
        public string CashCollectionRemark { get; set; } = string.Empty;
        public string Pin { get; set; } = string.Empty;
        public string ToDate { get; set; } = string.Empty;
    }
}
