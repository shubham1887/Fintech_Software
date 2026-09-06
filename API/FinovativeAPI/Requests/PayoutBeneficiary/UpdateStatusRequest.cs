using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.PayoutBeneficiary
{
    public class UpdateStatusRequest
    {
        private int _BeneficiaryID;

        public int BeneficiaryID
        {
            get => _BeneficiaryID;
            set => _BeneficiaryID = value;
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new UpdateStatusRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class UpdateStatusRequestValidator : AbstractValidator<UpdateStatusRequest>
    {
        public UpdateStatusRequestValidator()
        {
            RuleFor(x => x.BeneficiaryID)
               .Must(new Validation().IsValidInt)
               .WithMessage(SenderMessage.InvalidBeneficiary);
        }
    }
}