namespace FintechApp.Responses.WrongPasswordHistory {
    public class WrongPasswordHistoryResponse {
        public int WrongPasswordHistoryID { get; set; }
        public int UserID { get; set; }
        public string MobileNumber { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
