using FluentValidation;
namespace FintechApp.Requests.APIUserCallbackRequestResponseLog
{
    public class UpdateAPIUserCallbackRequestResponseLogRequest { public int APIUserCallbackRequestResponseLogID { get; set; } public int UserID { get; set; } public string Request { get; set; } = string.Empty; public string Response { get; set; } = string.Empty; public int TransactionMasterID { get; set; } public string APIUserRequestID { get; set; } = string.Empty; public string ConsumerNumber { get; set; } = string.Empty; }
    public class UpdateAPIUserCallbackRequestResponseLogRequestValidator : AbstractValidator<UpdateAPIUserCallbackRequestResponseLogRequest> { public UpdateAPIUserCallbackRequestResponseLogRequestValidator() { RuleFor(x => x.APIUserCallbackRequestResponseLogID).GreaterThan(0); } }
}
