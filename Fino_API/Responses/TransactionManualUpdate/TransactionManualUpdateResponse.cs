namespace FintechApp.Responses.TransactionManualUpdate {
    public class TransactionManualUpdateResponse {
        public int TransactionManualUpdateID { get; set; }
        public int TransactionMasterID { get; set; }
        public int ManualUpdatedBy { get; set; }
        public string ProviderrefNo { get; set; } = string.Empty;
        public string OldStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
