namespace FintechApp.Entities
{
    public class SmsTransactionMaster : BaseEntity
    {
        public int SmsTransactionMasterID { get; set; }
        public int TransactionMasterID { get; set; }
        public int ServiceID { get; set; }
        public decimal Amount { get; set; }
        public string BillNumber { get; set; } = string.Empty;
        public bool IsProcess { get; set; }
        public bool IsSuccess { get; set; }
        public TransactionMaster TransactionMaster { get; set; } = null!;
        public ServiceMaster ServiceMaster { get; set; } = null!;
    }
}
