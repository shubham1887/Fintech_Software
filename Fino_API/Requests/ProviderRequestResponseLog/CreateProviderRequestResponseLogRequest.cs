using FluentValidation;
namespace FintechApp.Requests.ProviderRequestResponseLog
{
    public class CreateProviderRequestResponseLogRequest { public int UserID { get; set; } public int ProviderID { get; set; } public int TransactionID { get; set; } public string Request { get; set; } = string.Empty; public string Response { get; set; } = string.Empty; public string IPAddress { get; set; } = string.Empty; public string Through { get; set; } = string.Empty; public int ServiceID { get; set; } }
    public class CreateProviderRequestResponseLogRequestValidator : AbstractValidator<CreateProviderRequestResponseLogRequest> { public CreateProviderRequestResponseLogRequestValidator() { RuleFor(x => x.UserID).GreaterThan(0); } }
}
