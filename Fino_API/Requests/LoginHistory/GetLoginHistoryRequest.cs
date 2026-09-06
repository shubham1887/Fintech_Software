namespace FintechApp.Requests.LoginHistory
{
    public class GetLoginHistoryRequest
    {
        public int UserID { get; set; }
        public string Through { get; set; } = string.Empty;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
