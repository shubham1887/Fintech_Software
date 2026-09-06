using FluentValidation;
namespace FintechApp.Requests.ProviderConfigurationAdminCommission
{
    public class DeleteProviderConfigurationAdminCommissionRequest { public int ProviderConfigurationAdminCommissionID { get; set; } }
    public class DeleteProviderConfigurationAdminCommissionRequestValidator : AbstractValidator<DeleteProviderConfigurationAdminCommissionRequest> { public DeleteProviderConfigurationAdminCommissionRequestValidator() { RuleFor(x => x.ProviderConfigurationAdminCommissionID).GreaterThan(0); } }
}
