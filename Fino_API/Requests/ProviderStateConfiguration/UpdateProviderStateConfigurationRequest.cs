using FluentValidation;
namespace FintechApp.Requests.ProviderStateConfiguration
{
    public class UpdateProviderStateConfigurationRequest { public int ProviderStateConfigurationID { get; set; } public int ProviderID { get; set; } public int StateID { get; set; } public string StateCode { get; set; } = string.Empty; }
    public class UpdateProviderStateConfigurationRequestValidator : AbstractValidator<UpdateProviderStateConfigurationRequest> { public UpdateProviderStateConfigurationRequestValidator() { RuleFor(x => x.ProviderStateConfigurationID).GreaterThan(0); } }
}
