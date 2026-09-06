namespace FintechApp.Responses.LapuTransactionDetail {
    public class LapuTransactionDetailResponse {
        public int LapuTransactionDetailID { get; set; }
        public int UserID { get; set; }
        public int OperatorID { get; set; }
        public int ProivderID { get; set; }
        public int LapuMasterID { get; set; }
        public int LapuPartyMasterID { get; set; }
        public string LapuNo { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ConsumerNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
