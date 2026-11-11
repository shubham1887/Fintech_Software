using FluentValidation;
namespace FintechApp.Requests.APIUserInitialRequestResponseLog
{
    public class GetAPIUserInitialRequestResponseLogRequest { public int APIUserInitialRequestResponseLogID { get; set; }
    public int UserID { get; set; }
    public int TransactionMasterID { get; set; } }
    public class GetAPIUserInitialRequestResponseLogRequestValidator : AbstractValidator<GetAPIUserInitialRequestResponseLogRequest> { public GetAPIUserInitialRequestResponseLogRequestValidator() { RuleFor(x => x.APIUserInitialRequestResponseLogID).GreaterThan(0); } }
}
