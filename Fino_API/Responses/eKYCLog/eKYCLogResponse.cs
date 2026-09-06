namespace FintechApp.Responses.eKYCLog {
    public class eKYCLogResponse {
        public int eKYCLogID { get; set; }
        public string DocumentNo { get; set; } = string.Empty;
        public int UserID { get; set; }
        public string Response { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
