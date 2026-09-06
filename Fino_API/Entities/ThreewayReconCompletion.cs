namespace FintechApp.Entities
{
    public class ThreewayReconCompletion : BaseEntity
    {
        public int ThreewayReconCompletionID { get; set; }
        public int TransactionMasterID { get; set; }
        public int ServiceID { get; set; }
        public int OperatorID { get; set; }
        public TransactionMaster TransactionMaster { get; set; } = null!;
        public ServiceMaster ServiceMaster { get; set; } = null!;
        public OperatorMaster OperatorMaster { get; set; } = null!;
    }
}
