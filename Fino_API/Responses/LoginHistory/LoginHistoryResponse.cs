namespace FintechApp.Responses.LoginHistory
{
    public class LoginHistoryResponse
    {
        public int LoginHistoryID { get; set; }
        public int UserID { get; set; }
        public string IPAddress { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Through { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string LoginMessage { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string SimCardDetails { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
