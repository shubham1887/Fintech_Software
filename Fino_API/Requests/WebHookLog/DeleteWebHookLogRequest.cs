using FluentValidation;
namespace FintechApp.Requests.WebHookLog
{
    public class DeleteWebHookLogRequest { public int WebHookLogID { get; set; } }
    public class DeleteWebHookLogRequestValidator : AbstractValidator<DeleteWebHookLogRequest> { public DeleteWebHookLogRequestValidator() { RuleFor(x => x.WebHookLogID).GreaterThan(0); } }
}
