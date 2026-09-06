namespace FintechApp.Requests.Fund
{
    public class FundInsertRequest
    {
        public decimal Amount { get; set; }
        public int WalletID { get; set; }
        public string Remark { get; set; } = string.Empty;
        public string UTRNumber { get; set; } = string.Empty;
        public string PayMode { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string TransactionDate { get; set; } = string.Empty;
        public string Document { get; set; } = string.Empty;
        public int RequestTo { get; set; }
        // New master-driven method + source/destination. Optional so the legacy PayMode/BankName
        // path stays valid; when supplied, the selected Sub Mode must belong to the selected Mode.
        public int ModeID { get; set; }
        public int SubModeID { get; set; }
    }
}
