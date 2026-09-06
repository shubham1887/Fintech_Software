namespace FintechApp.Entities
{
    public class TransactionManualUpdate : BaseEntity
    {
        public int TransactionManualUpdateID { get; set; }
        public int TransactionMasterID { get; set; }
        public int ManualUpdatedBy { get; set; }
        public string ProviderrefNo { get; set; } = string.Empty;
        public string OldStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
        public TransactionMaster TransactionMaster { get; set; } = null!;
        public UserMaster UserMaster { get; set; } = null!;
    }
}
