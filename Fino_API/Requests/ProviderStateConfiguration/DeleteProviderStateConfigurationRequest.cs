using FluentValidation;
namespace FintechApp.Requests.ProviderStateConfiguration
{
    public class DeleteProviderStateConfigurationRequest { public int ProviderStateConfigurationID { get; set; } }
    public class DeleteProviderStateConfigurationRequestValidator : AbstractValidator<DeleteProviderStateConfigurationRequest> { public DeleteProviderStateConfigurationRequestValidator() { RuleFor(x => x.ProviderStateConfigurationID).GreaterThan(0); } }
}
