namespace FintechApp.Entities
{
    public class TargetSaleCommission : BaseEntity
    {
        public int TargetSaleCommissionID { get; set; }
        public int UserID { get; set; }
        public int WalletID { get; set; }
        public int ParentID { get; set; }
        public decimal SaleAmount { get; set; }
        public decimal CommissionAmount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string SaleMonthDOC { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public UserMaster UserMaster { get; set; } = null!;
        public WalletMaster WalletMaster { get; set; } = null!;
    }
}
