using FluentValidation;
namespace FintechApp.Requests.ProviderStateConfiguration
{
    public class CreateProviderStateConfigurationRequest { public int ProviderID { get; set; } public int StateID { get; set; } public string StateCode { get; set; } = string.Empty; }
    public class CreateProviderStateConfigurationRequestValidator : AbstractValidator<CreateProviderStateConfigurationRequest> { public CreateProviderStateConfigurationRequestValidator() { RuleFor(x => x.ProviderID).GreaterThan(0); RuleFor(x => x.StateCode).NotEmpty(); } }
}
