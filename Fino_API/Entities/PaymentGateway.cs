namespace FintechApp.Entities
{
    public class PaymentGateway : BaseEntity
    {
        public int PaymentGatewayID { get; set; }
        public int UserID { get; set; }
        public string OrderID { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal GatewayCharge { get; set; }
        public decimal Commission { get; set; }
        public int WalletID { get; set; }
        public string Status { get; set; } = string.Empty;
        public string TransactionID { get; set; } = string.Empty;
        public string PayMode { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
        public int PaymentProviderID { get; set; }
        public int IsUpdate { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public string Remark { get; set; } = string.Empty;
        public int DistributerUserID { get; set; }
        public int SuperDistributerUserID { get; set; }
        public decimal DistributerCommission { get; set; }
        public decimal SuperDistributerCommission { get; set; }
        public decimal RetailerTDS { get; set; }
        public decimal DistributerTDS { get; set; }
        public decimal SuperDistributerTDS { get; set; }
        public decimal AdminGST { get; set; }
        public string ReferenceID { get; set; } = string.Empty;
        public int ServiceID { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public UserMaster UserMaster { get; set; } = null!;
        public ServiceMaster ServiceMaster { get; set; } = null!;
    }
}
