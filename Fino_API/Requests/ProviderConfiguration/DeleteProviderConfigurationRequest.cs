using FluentValidation;

namespace FintechApp.Requests.ProviderConfiguration
{
    public class DeleteProviderConfigurationRequest
    {
        public int ConfigurationID { get; set; }
    }

    public class DeleteProviderConfigurationRequestValidator : AbstractValidator<DeleteProviderConfigurationRequest>
    {
        public DeleteProviderConfigurationRequestValidator()
        {
            RuleFor(x => x.ConfigurationID).GreaterThan(0);
        }
    }
}
