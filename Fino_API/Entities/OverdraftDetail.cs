namespace FintechApp.Entities
{
    public class OverdraftDetail : BaseEntity
    {
        public int OverdraftDetailID { get; set; }
        public int UserID { get; set; }
        public int WalletID { get; set; }
        public decimal OverdraftOpening { get; set; }
        public decimal OverdraftAmount { get; set; }
        public decimal OverdraftClosing { get; set; }
        public string UserRemark { get; set; } = string.Empty;
        public string AdminRemark { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal OpeningWalletBalance { get; set; }
        public UserMaster UserMaster { get; set; } = null!;
        public WalletMaster WalletMaster { get; set; } = null!;
    }
}
