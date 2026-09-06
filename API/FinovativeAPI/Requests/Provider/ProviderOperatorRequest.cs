using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Provider
{
    public class ProviderOperatorRequest
    {
        private int _ProviderOperatorID;
        private int _ProviderID;
        private bool _IsActive;
        public int ProviderOperatorID { get => _ProviderOperatorID; set => _ProviderOperatorID = value; }
        public bool IsActive { get => _IsActive; set => _IsActive = value; }
        public int ProviderID { get => _ProviderID; set => _ProviderID = value; }

        internal string IsValidModalState()
        {
            var validator = new ProviderOperatorValidator();
            FluentValidation.Results.ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class ProviderOperatorValidator : AbstractValidator<ProviderOperatorRequest>
    {
        public ProviderOperatorValidator()
        {
            RuleFor(x => x.ProviderOperatorID)
              .Must(new Validation().IsValidInt)
              .WithMessage(Messages.InValidProviderOperatorID);
        }
    }
}