using FluentValidation;
namespace FintechApp.Requests.APIUserInitialRequestResponseLog
{
    public class DeleteAPIUserInitialRequestResponseLogRequest { public int APIUserInitialRequestResponseLogID { get; set; } }
    public class DeleteAPIUserInitialRequestResponseLogRequestValidator : AbstractValidator<DeleteAPIUserInitialRequestResponseLogRequest> { public DeleteAPIUserInitialRequestResponseLogRequestValidator() { RuleFor(x => x.APIUserInitialRequestResponseLogID).GreaterThan(0); } }
}
