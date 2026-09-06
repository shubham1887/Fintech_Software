using FluentValidation;

namespace FintechApp.Requests.ProviderGateway
{
    public class ProviderBalanceRequest
    {
        public int ProviderID { get; set; }
    }

    public class ProviderBalanceRequestValidator : AbstractValidator<ProviderBalanceRequest>
    {
        public ProviderBalanceRequestValidator()
        {
            RuleFor(x => x.ProviderID).GreaterThan(0);
        }
    }
}
