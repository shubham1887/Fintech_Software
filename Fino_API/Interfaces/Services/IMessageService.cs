namespace FintechApp.Interfaces.Services
{
    public interface IMessageService
    {
        /// <summary>
        /// Central, settings-driven sender. Looks up the admin-configured Message Setting template
        /// for <paramref name="eventType"/> (matched loosely so UI names like "Login OTP" resolve
        /// against code events like "LoginOTP"), fills its {Token}/[Token] placeholders from
        /// <paramref name="tokens"/>, and delivers over every channel the template has switched on
        /// (SMS / WhatsApp / Email). When no template exists it falls back to <paramref name="fallbackSms"/>
        /// so a message always goes out. Fire-and-forget: never throws.
        /// </summary>
        Task SendEventAsync(string eventType, int userId, string mobile, string? whatsAppNumber,
            string? emailId, IDictionary<string, string>? tokens, string fallbackSms,
            bool isOtpDefault = false, string emailSubject = "");

        Task SendSmsAsync(int userId, string mobile, string message, string templateId, string type);
        Task SendOtpSmsAsync(int userId, string mobile, string message, string templateId, string type);
        Task SendWhatsAppAsync(int userId, string mobile, string whatsAppNumber, string message, string templateId, string type);
        Task SendEmailAsync(string emailId, string message, string subject, string attachment = "");
        Task<string> CheckSmsAsync(string mobile, string message, string templateId);
        Task<string> CheckEmailAsync(string emailId, string message, string subject);
        Task<string> CheckWhatsAppAsync(string whatsAppNumber, string message);
    }
}
