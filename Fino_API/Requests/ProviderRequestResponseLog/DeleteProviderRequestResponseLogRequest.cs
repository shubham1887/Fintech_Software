using FluentValidation;
namespace FintechApp.Requests.ProviderRequestResponseLog
{
    public class DeleteProviderRequestResponseLogRequest { public int ProviderRequestResponseLogID { get; set; } }
    public class DeleteProviderRequestResponseLogRequestValidator : AbstractValidator<DeleteProviderRequestResponseLogRequest> { public DeleteProviderRequestResponseLogRequestValidator() { RuleFor(x => x.ProviderRequestResponseLogID).GreaterThan(0); } }
}
