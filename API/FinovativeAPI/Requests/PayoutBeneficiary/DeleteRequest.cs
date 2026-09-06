using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.PayoutBeneficiary
{
    public class DeleteRequest
    {
        private int _BeneficiaryID;

        public int BeneficiaryID
        {
            get => _BeneficiaryID;
            set => _BeneficiaryID = value;
        }

        private string _ActivePayoutName;

        public string ActivePayoutName
        {
            get => _ActivePayoutName.TrimIfNotNull();
            set => _ActivePayoutName = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new DeleteRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class DeleteRequestValidator : AbstractValidator<DeleteRequest>
    {
        public DeleteRequestValidator()
        {
            RuleFor(x => x.BeneficiaryID)
               .Must(new Validation().IsValidInt)
               .WithMessage(SenderMessage.InvalidBeneficiary);
        }
    }
}