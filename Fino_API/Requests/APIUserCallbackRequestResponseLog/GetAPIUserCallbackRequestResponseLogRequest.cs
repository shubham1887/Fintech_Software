using FluentValidation;
namespace FintechApp.Requests.APIUserCallbackRequestResponseLog
{
    public class GetAPIUserCallbackRequestResponseLogRequest { public int APIUserCallbackRequestResponseLogID { get; set; } public int UserID { get; set; } }
    public class GetAPIUserCallbackRequestResponseLogRequestValidator : AbstractValidator<GetAPIUserCallbackRequestResponseLogRequest> { public GetAPIUserCallbackRequestResponseLogRequestValidator() { RuleFor(x => x.APIUserCallbackRequestResponseLogID).GreaterThan(0); } }
}
