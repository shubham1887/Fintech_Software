namespace FintechApp.Entities
{
    public class TransactionDetails : BaseEntity
    {
        public int TransactionDetailID { get; set; }
        public int UserID { get; set; }
        public int ReferenceID { get; set; }
        public int AssignedUserID { get; set; }
        public int OperatorID { get; set; }
        public int ServiceID { get; set; }
        public int WalletID { get; set; }
        public string DOCType { get; set; } = string.Empty;
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public decimal TransactionAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal GST { get; set; }
        public decimal TDS { get; set; }
        public string AmountCreditDebit { get; set; } = string.Empty;
        public string CommissionCreditDebit { get; set; } = string.Empty;
        public string TransactionDescription { get; set; } = string.Empty;
        public string ConsumerNumber { get; set; } = string.Empty;
        public string PayMode { get; set; } = string.Empty;
    }
}
