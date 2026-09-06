using FluentValidation;
namespace FintechApp.Requests.SentSMS
{
    public class GetSentSMSRequest { public int SentSMSID { get; set; }
    public int UserID { get; set; } }
    public class GetSentSMSRequestValidator : AbstractValidator<GetSentSMSRequest> { public GetSentSMSRequestValidator() { RuleFor(x => x.SentSMSID).GreaterThan(0); } }
}
