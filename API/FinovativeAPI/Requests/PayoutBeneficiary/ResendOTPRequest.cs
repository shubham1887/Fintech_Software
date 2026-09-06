using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.PayoutBeneficiary
{
    public class ResendOTPRequest
    {
        private int _BeneficiaryID;

        public int BeneficiaryID
        {
            get => _BeneficiaryID;
            set => _BeneficiaryID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new ResendOTPRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class ResendOTPRequestValidator : AbstractValidator<ResendOTPRequest>
    {
        public ResendOTPRequestValidator()
        {
            RuleFor(x => x.BeneficiaryID)
                .Must(new Validation().IsValidInt)
                .WithMessage(BeneficiaryMessage.InvalidID);
        }
    }
}