namespace FintechApp.Entities
{
    public class TransactionStatusChange : BaseEntity
    {
        public int TransactionStatusChangeID { get; set; }
        public int UserID { get; set; }
        public int TransactionMasterID { get; set; }
        public int TransactionDOCInt { get; set; }
        public string OldStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Remark { get; set; } = string.Empty;
        public UserMaster UserMaster { get; set; } = null!;
        public TransactionMaster TransactionMaster { get; set; } = null!;
    }
}
