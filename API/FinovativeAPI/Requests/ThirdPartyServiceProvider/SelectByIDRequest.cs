using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;

namespace Project.Requests.ThirdPartyServiceProvider
{
    public class SelectByIDRequest
    {
        private int _ThirdPartyServiceProviderID;
        public int ThirdPartyServiceProviderID { get => _ThirdPartyServiceProviderID; set => _ThirdPartyServiceProviderID = value; }

        internal string IsValidModalState()
        {
            var validator = new SelectByIDRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            System.Collections.Generic.IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectByIDRequestValidator : AbstractValidator<SelectByIDRequest>
    {
        public SelectByIDRequestValidator()
        {
            RuleFor(config => config.ThirdPartyServiceProviderID)
                .Must(new Validation().IsValidInt)
                .WithMessage(ThirdPartyServiceProviderMessages.InvalidSetting);
        }
    }
}