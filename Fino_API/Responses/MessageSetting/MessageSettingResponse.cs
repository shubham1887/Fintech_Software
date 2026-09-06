namespace FintechApp.Responses.MessageSetting
{
    public class MessageSettingResponse
    {
        public int MessageSettingID { get; set; }
        public string Type { get; set; } = string.Empty;
        public bool IsSMS { get; set; }
        public bool IsMail { get; set; }
        public bool IsWhatsUp { get; set; }
        public string WhatsUpMessage { get; set; } = string.Empty;
        public string SmsMessage { get; set; } = string.Empty;
        public string EmailMessage { get; set; } = string.Empty;
        public string TemplateID { get; set; } = string.Empty;
        public bool IsOTP { get; set; }
        public DateTime DateTime { get; set; }
    }
}
