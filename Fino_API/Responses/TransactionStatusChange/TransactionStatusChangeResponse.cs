namespace FintechApp.Responses.TransactionStatusChange {
    public class TransactionStatusChangeResponse {
        public int TransactionStatusChangeID { get; set; }
        public int UserID { get; set; }
        public int TransactionMasterID { get; set; }
        public int TransactionDOCInt { get; set; }
        public string OldStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Remark { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
