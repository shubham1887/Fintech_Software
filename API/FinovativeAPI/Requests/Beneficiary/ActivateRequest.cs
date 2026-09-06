using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Beneficiary
{
    public class ActivateRequest
    {
        private int _BeneficiaryID;

        public int BeneficiaryID
        {
            get => _BeneficiaryID;
            set => _BeneficiaryID = value;
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        internal string IsValidModalState()
        {
            var validator = new ActivateRequestValidator();
            ValidationResult results = validator.Validate(this);
            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class ActivateRequestValidator : AbstractValidator<ActivateRequest>
    {
        public ActivateRequestValidator()
        {
            RuleFor(x => x.BeneficiaryID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.BeneficiaryMessage.InvalidBeneficiary);
        }
    }
}