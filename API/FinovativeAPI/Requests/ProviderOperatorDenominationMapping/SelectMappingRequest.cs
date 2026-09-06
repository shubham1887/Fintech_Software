using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.ProviderOperatorDenominationMapping
{
    public class SelectMappingRequest
    {
        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SelectMappingRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectMappingRequestValidator : AbstractValidator<SelectMappingRequest>
    {
        public SelectMappingRequestValidator()
        {
            RuleFor(x => x.ServiceID)
                          .Must(new Validation().IsValidInt)
                          .WithMessage(Message.ServiceMessage.InvalidServideID);
        }
    }
}