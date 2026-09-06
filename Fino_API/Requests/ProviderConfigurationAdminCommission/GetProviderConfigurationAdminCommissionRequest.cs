using FluentValidation;
namespace FintechApp.Requests.ProviderConfigurationAdminCommission
{
    public class GetProviderConfigurationAdminCommissionRequest { public int ProviderConfigurationAdminCommissionID { get; set; }
    public int ProviderID { get; set; } }
    public class GetProviderConfigurationAdminCommissionRequestValidator : AbstractValidator<GetProviderConfigurationAdminCommissionRequest> { public GetProviderConfigurationAdminCommissionRequestValidator() { RuleFor(x => x.ProviderConfigurationAdminCommissionID).GreaterThan(0); } }
}
