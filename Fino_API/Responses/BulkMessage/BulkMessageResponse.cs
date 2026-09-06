namespace FintechApp.Responses.BulkMessage
{
    public class BulkMessageResponse
    {
    public int BulkMessageID { get; set; }
    public int UserID { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsSMS { get; set; }
    public bool IsEmail { get; set; }
    public bool IsWhatsUp { get; set; }
    public bool IsDelete { get; set; }
    public DateTime DateTime { get; set; } = DateTime.MinValue;
    }
}
