using FluentValidation;
namespace FintechApp.Requests.SentSMS
{
    public class DeleteSentSMSRequest { public int SentSMSID { get; set; } }
    public class DeleteSentSMSRequestValidator : AbstractValidator<DeleteSentSMSRequest> { public DeleteSentSMSRequestValidator() { RuleFor(x => x.SentSMSID).GreaterThan(0); } }
}
