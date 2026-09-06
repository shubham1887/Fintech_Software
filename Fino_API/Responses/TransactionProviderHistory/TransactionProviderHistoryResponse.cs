namespace FintechApp.Responses.TransactionProviderHistory {
    public class TransactionProviderHistoryResponse {
        public int RechargeProviderHistoryID { get; set; }
        public int ProviderID { get; set; }
        public int OperatorID { get; set; }
        public int TransactionMasterID { get; set; }
        public string ProviderRequestID { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
