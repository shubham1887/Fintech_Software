namespace FintechApp.Requests.APIUserInitialRequestResponseLog
{
    /// <summary>The API Partner Logs screen's filters. Dates are "yyyy-MM-dd"; 0 means "any".</summary>
    public class SelectAPIUserLogRequest
    {
        public string FromDate { get; set; } = string.Empty;
        public string ToDate { get; set; } = string.Empty;
        public int UserID { get; set; }
        public int ServiceID { get; set; }
        public int OperatorID { get; set; }
        public string Search { get; set; } = string.Empty;
        public int SkipCount { get; set; }
        public int TakeCount { get; set; }
    }
}
