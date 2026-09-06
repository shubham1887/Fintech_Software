using FluentValidation;
namespace FintechApp.Requests.ProviderRequestResponseLog
{
    public class UpdateProviderRequestResponseLogRequest { public int ProviderRequestResponseLogID { get; set; } public int UserID { get; set; } public int ProviderID { get; set; } public int TransactionID { get; set; } public string Request { get; set; } = string.Empty; public string Response { get; set; } = string.Empty; public string IPAddress { get; set; } = string.Empty; public string Through { get; set; } = string.Empty; public int ServiceID { get; set; } }
    public class UpdateProviderRequestResponseLogRequestValidator : AbstractValidator<UpdateProviderRequestResponseLogRequest> { public UpdateProviderRequestResponseLogRequestValidator() { RuleFor(x => x.ProviderRequestResponseLogID).GreaterThan(0); } }
}
