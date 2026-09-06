namespace FintechApp.Responses.SentSMS
{
    public class SentSMSResponse
    {
    public int SentSMSID { get; set; }
    public int UserID { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public int SMSMasterID { get; set; }
    public string TemplateID { get; set; } = string.Empty;
    public string WhatsAppNumber { get; set; } = string.Empty;
    public string Request { get; set; } = string.Empty;
    public DateTime DateTime { get; set; } = DateTime.MinValue;
    }
}
