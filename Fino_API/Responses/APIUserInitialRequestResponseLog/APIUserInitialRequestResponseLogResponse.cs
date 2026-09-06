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
    }
}
