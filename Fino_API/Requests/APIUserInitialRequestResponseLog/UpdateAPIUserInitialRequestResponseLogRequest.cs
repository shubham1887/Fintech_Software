using FluentValidation;
namespace FintechApp.Requests.APIUserInitialRequestResponseLog
{
    public class UpdateAPIUserInitialRequestResponseLogRequest { public int APIUserInitialRequestResponseLogID { get; set; } public int UserID { get; set; } public string Request { get; set; } = string.Empty; public string Response { get; set; } = string.Empty; public int TransactionMasterID { get; set; } public string APIUserRequestID { get; set; } = string.Empty; public string ConsumerNumber { get; set; } = string.Empty; }
    public class UpdateAPIUserInitialRequestResponseLogRequestValidator : AbstractValidator<UpdateAPIUserInitialRequestResponseLogRequest> { public UpdateAPIUserInitialRequestResponseLogRequestValidator() { RuleFor(x => x.APIUserInitialRequestResponseLogID).GreaterThan(0); } }
}
