namespace FintechApp.Responses.SmsTransactionMaster {
    public class SmsTransactionMasterResponse {
        public int SmsTransactionMasterID { get; set; }
        public int TransactionMasterID { get; set; }
        public int ServiceID { get; set; }
        public decimal Amount { get; set; }
        public string BillNumber { get; set; } = string.Empty;
        public bool IsProcess { get; set; }
        public bool IsSuccess { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
