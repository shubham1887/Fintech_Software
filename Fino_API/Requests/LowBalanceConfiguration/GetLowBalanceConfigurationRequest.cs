using FluentValidation;

namespace FintechApp.Requests.LowBalanceConfiguration
{
    public class GetLowBalanceConfigurationRequest
    {
        public int LowBalanceConfigurationID { get; set; }
        public int UserID { get; set; }
    }

    public class GetLowBalanceConfigurationRequestValidator : AbstractValidator<GetLowBalanceConfigurationRequest>
    {
        public GetLowBalanceConfigurationRequestValidator()
        {
            RuleFor(x => x.LowBalanceConfigurationID).GreaterThan(0);
        }
    }
}
