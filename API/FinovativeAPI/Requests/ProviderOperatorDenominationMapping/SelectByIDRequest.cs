using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.ProviderOperatorDenominationMapping
{
    public class SelectByIDRequest
    {
        private int _ProviderOperatorDenominationMappingID;

        public int ProviderOperatorDenominationMappingID
        {
            get => _ProviderOperatorDenominationMappingID;
            set => _ProviderOperatorDenominationMappingID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SelectByIDRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class SelectByIDRequestValidator : AbstractValidator<SelectByIDRequest>
    {
        public SelectByIDRequestValidator()
        {
            RuleFor(x => x.ProviderOperatorDenominationMappingID)
                          .Must(new Validation().IsValidInt)
                          .WithMessage(Message.ProviderOperatorDenominationMappingMessage.NotFound);
        }
    }
}