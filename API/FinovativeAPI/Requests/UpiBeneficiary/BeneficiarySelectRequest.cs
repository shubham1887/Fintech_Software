using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.UpiBeneficiary
{
    public class BeneficiarySelectRequest
    {
        private string _UpiBeneficiaryID;

        public string UpiBeneficiaryID
        {
            get => _UpiBeneficiaryID.TrimIfNotNull();
            set => _UpiBeneficiaryID = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new BeneficiarySelectRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class BeneficiarySelectRequestValidator : AbstractValidator<BeneficiarySelectRequest>
    {
        public BeneficiarySelectRequestValidator()
        {
            RuleFor(x => x.UpiBeneficiaryID)
               .Must(new Validation().IsValidString)
               .WithMessage(SenderMessage.InvalidSender);
        }
    }
}