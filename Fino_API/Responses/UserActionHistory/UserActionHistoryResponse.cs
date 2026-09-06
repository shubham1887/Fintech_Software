namespace FintechApp.Responses.UserActionHistory {
    public class UserActionHistoryResponse {
        public int UserHistoryID { get; set; }
        public int UserID { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
