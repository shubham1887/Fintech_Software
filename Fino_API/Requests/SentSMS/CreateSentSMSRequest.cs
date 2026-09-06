using FluentValidation;
namespace FintechApp.Requests.SentSMS
{
    public class CreateSentSMSRequest { public int UserID { get; set; } public string Mobile { get; set; } = string.Empty; public string Message { get; set; } = string.Empty; public string Type { get; set; } = string.Empty; public string Response { get; set; } = string.Empty; public int SMSMasterID { get; set; } public string TemplateID { get; set; } = string.Empty; public string WhatsAppNumber { get; set; } = string.Empty; public string Request { get; set; } = string.Empty; }
    public class CreateSentSMSRequestValidator : AbstractValidator<CreateSentSMSRequest> { public CreateSentSMSRequestValidator() { RuleFor(x => x.UserID).GreaterThan(0); } }
}
