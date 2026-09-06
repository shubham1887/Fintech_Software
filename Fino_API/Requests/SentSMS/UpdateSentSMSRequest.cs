using FluentValidation;
namespace FintechApp.Requests.SentSMS
{
    public class UpdateSentSMSRequest { public int SentSMSID { get; set; } public int UserID { get; set; } public string Mobile { get; set; } = string.Empty; public string Message { get; set; } = string.Empty; public string Type { get; set; } = string.Empty; public string Response { get; set; } = string.Empty; public int SMSMasterID { get; set; } public string TemplateID { get; set; } = string.Empty; public string WhatsAppNumber { get; set; } = string.Empty; public string Request { get; set; } = string.Empty; }
    public class UpdateSentSMSRequestValidator : AbstractValidator<UpdateSentSMSRequest> { public UpdateSentSMSRequestValidator() { RuleFor(x => x.SentSMSID).GreaterThan(0); } }
}
