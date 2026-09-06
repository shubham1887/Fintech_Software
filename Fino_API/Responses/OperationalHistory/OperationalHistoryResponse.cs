namespace FintechApp.Responses.OperationalHistory {
    public class OperationalHistoryResponse {
        public int OperationalHistoryID { get; set; }
        public int UserID { get; set; }
        public string APIType { get; set; } = string.Empty;
        public string DocNo { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
