namespace FintechApp.Responses.APIUserInitialRequestResponseLog
{
    public class APIUserInitialRequestResponseLogResponse
    {
    public int APIUserInitialRequestResponseLogID { get; set; }
    public int UserID { get; set; }
    public string Request { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public int TransactionMasterID { get; set; }
    public string APIUserRequestID { get; set; } = string.Empty;
    public string ConsumerNumber { get; set; } = string.Empty;
    public DateTime DateTime { get; set; } = DateTime.MinValue;
    // Resolved for the log screen: who called, and what the call became.
    public string UserName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string OperatorName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    }

    public class APIUserLogWrapper
    {
        public int Count { get; set; }
        public List<APIUserInitialRequestResponseLogResponse> list { get; set; } = new();
    }
}
