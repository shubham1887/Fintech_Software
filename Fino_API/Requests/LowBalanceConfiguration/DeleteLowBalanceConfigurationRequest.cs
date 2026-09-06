using FluentValidation;

namespace FintechApp.Requests.LowBalanceConfiguration
{
    public class DeleteLowBalanceConfigurationRequest
    {
        public int LowBalanceConfigurationID { get; set; }
    }

    public class DeleteLowBalanceConfigurationRequestValidator : AbstractValidator<DeleteLowBalanceConfigurationRequest>
    {
        public DeleteLowBalanceConfigurationRequestValidator()
        {
            RuleFor(x => x.LowBalanceConfigurationID).GreaterThan(0);
        }
    }
}
