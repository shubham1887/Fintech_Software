using FluentValidation;
namespace FintechApp.Requests.APIUserCallbackRequestResponseLog
{
    public class CreateAPIUserCallbackRequestResponseLogRequest { public int UserID { get; set; } public string Request { get; set; } = string.Empty; public string Response { get; set; } = string.Empty; public int TransactionMasterID { get; set; } public string APIUserRequestID { get; set; } = string.Empty; public string ConsumerNumber { get; set; } = string.Empty; }
    public class CreateAPIUserCallbackRequestResponseLogRequestValidator : AbstractValidator<CreateAPIUserCallbackRequestResponseLogRequest> { public CreateAPIUserCallbackRequestResponseLogRequestValidator() { RuleFor(x => x.UserID).GreaterThan(0); } }
}
