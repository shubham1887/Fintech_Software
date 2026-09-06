namespace FintechApp.Responses.TargetSaleCommission {
    public class TargetSaleCommissionResponse {
        public int TargetSaleCommissionID { get; set; }
        public int UserID { get; set; }
        public int WalletID { get; set; }
        public int ParentID { get; set; }
        public decimal SaleAmount { get; set; }
        public decimal CommissionAmount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string SaleMonthDOC { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
