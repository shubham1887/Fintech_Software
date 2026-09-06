namespace FintechApp.Entities
{
    public class TransactionProviderHistory : BaseEntity
    {
        public int RechargeProviderHistoryID { get; set; }
        public int ProviderID { get; set; }
        public int OperatorID { get; set; }
        public int TransactionMasterID { get; set; }
        public string ProviderRequestID { get; set; } = string.Empty;
        public TransactionMaster TransactionMaster { get; set; } = null!;
    }
}
