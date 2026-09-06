using FluentValidation;
namespace FintechApp.Requests.ProviderStateConfiguration
{
    public class GetProviderStateConfigurationRequest { public int ProviderStateConfigurationID { get; set; } public int ProviderID { get; set; } }
    public class GetProviderStateConfigurationRequestValidator : AbstractValidator<GetProviderStateConfigurationRequest> { public GetProviderStateConfigurationRequestValidator() { RuleFor(x => x.ProviderStateConfigurationID).GreaterThan(0); } }
}
