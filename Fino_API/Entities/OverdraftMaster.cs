namespace FintechApp.Entities
{
    public class OverdraftMaster : BaseEntity
    {
        public int OverdraftMasterID { get; set; }
        public int UserID { get; set; }
        public decimal Overdraft { get; set; }
        public decimal MaxOverdraft { get; set; }
        public int WalletID { get; set; }
        public bool IsActive { get; set; }
        public UserMaster UserMaster { get; set; } = null!;
        public WalletMaster WalletMaster { get; set; } = null!;
    }
}
