namespace FintechApp.Responses.Inbox
{
    public class InboxResponse
    {
    public int InboxID { get; set; }
    public int ServiceID { get; set; }
    public int OperatorID { get; set; }
    public int UserID { get; set; }
    public decimal Amount { get; set; }
    public string ConsumerNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Through { get; set; } = string.Empty;
    public string IPAddress { get; set; } = string.Empty;
    public string Param1 { get; set; } = string.Empty;
    public DateTime DateTime { get; set; } = DateTime.MinValue;
    }
}
