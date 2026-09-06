using FluentValidation;

namespace FintechApp.Requests.ProviderDetails
{
    public class DeleteProviderDetailsRequest
    {
        public int ProviderDetailID { get; set; }
    }

    public class DeleteProviderDetailsRequestValidator : AbstractValidator<DeleteProviderDetailsRequest>
    {
        public DeleteProviderDetailsRequestValidator()
        {
            RuleFor(x => x.ProviderDetailID).GreaterThan(0);
        }
    }
}
