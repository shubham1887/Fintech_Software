namespace FintechApp.Responses.ProviderRequestResponseLog
{
    public class ProviderRequestResponseLogResponse
    {
    public int ProviderRequestResponseLogID { get; set; }
    public int UserID { get; set; }
    public int ProviderID { get; set; }
    public int TransactionID { get; set; }
    public string Request { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public string IPAddress { get; set; } = string.Empty;
    public string Through { get; set; } = string.Empty;
    public int ServiceID { get; set; }
    public DateTime DateTime { get; set; } = DateTime.MinValue;
    // Display names resolved during enrichment (the log table shows names, not raw ids).
    public string UserName { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    }
}
