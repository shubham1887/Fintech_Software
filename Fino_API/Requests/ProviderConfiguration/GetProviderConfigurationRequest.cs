using FluentValidation;

namespace FintechApp.Requests.ProviderConfiguration
{
    public class GetProviderConfigurationRequest
    {
        public int ConfigurationID { get; set; }
        public int ProviderID { get; set; }
        public int OperatorID { get; set; }
        public int ServiceID { get; set; }
    }

    public class GetProviderConfigurationRequestValidator : AbstractValidator<GetProviderConfigurationRequest>
    {
        public GetProviderConfigurationRequestValidator() { }
    }
}
