using FluentValidation;
namespace FintechApp.Requests.APIUserCallbackRequestResponseLog
{
    public class DeleteAPIUserCallbackRequestResponseLogRequest { public int APIUserCallbackRequestResponseLogID { get; set; } }
    public class DeleteAPIUserCallbackRequestResponseLogRequestValidator : AbstractValidator<DeleteAPIUserCallbackRequestResponseLogRequest> { public DeleteAPIUserCallbackRequestResponseLogRequestValidator() { RuleFor(x => x.APIUserCallbackRequestResponseLogID).GreaterThan(0); } }
}
