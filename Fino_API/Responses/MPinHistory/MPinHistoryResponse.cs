namespace FintechApp.Responses.MPinHistory {
    public class MPinHistoryResponse {
        public int MPinHistoryID { get; set; }
        public int UserID { get; set; }
        public int Mpin { get; set; }
        public string DeviceID { get; set; } = string.Empty;
        public string Device { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
