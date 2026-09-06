namespace FintechApp.Responses.VirtualBalance {
    public class VirtualBalanceResponse {
        public int VirtualBalanceID { get; set; }
        public int UserID { get; set; }
        public decimal Amount { get; set; }
        public int WalletID { get; set; }
        public string DOCType { get; set; } = string.Empty;
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public DateTime DateTime { get; set; }
        public string WalletName { get; set; } = string.Empty;
    }
}
