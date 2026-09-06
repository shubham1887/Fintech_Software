namespace FintechApp.Entities
{
    public class VirtualBalance
    {
        public int VirtualBalanceID { get; set; }

        // The admin who took this virtual balance for themselves; the credit lands in this
        // user's UserWalletMaster row for WalletID, from where it can be fund-transferred on.
        public int UserID { get; set; }

        public decimal Amount { get; set; }
        public int WalletID { get; set; }
        public string DOCType { get; set; } = string.Empty;
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }

        // When the balance was taken. Rows created before this column existed read as default.
        public DateTime DateTime { get; set; }
    }
}
