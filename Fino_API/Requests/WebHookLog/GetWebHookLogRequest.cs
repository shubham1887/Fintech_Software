using FluentValidation;
namespace FintechApp.Requests.WebHookLog
{
    public class GetWebHookLogRequest { public int WebHookLogID { get; set; } }
    public class GetWebHookLogRequestValidator : AbstractValidator<GetWebHookLogRequest> { public GetWebHookLogRequestValidator() { RuleFor(x => x.WebHookLogID).GreaterThan(0); } }
}
