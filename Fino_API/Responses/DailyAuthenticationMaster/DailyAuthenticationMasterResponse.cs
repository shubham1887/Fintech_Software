namespace FintechApp.Responses.DailyAuthenticationMaster {
    public class DailyAuthenticationMasterResponse {
        public int AuthenticationID { get; set; }
        public int UserID { get; set; }
        public int ProviderID { get; set; }
        public int ServiceID { get; set; }
        public int OperatorID { get; set; }
        public string Pipe { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
