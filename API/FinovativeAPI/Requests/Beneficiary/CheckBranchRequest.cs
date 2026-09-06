using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Beneficiary
{
    public class CheckBranchRequest
    {
        private string _IFSC;

        public string IFSC
        {
            get => _IFSC.TrimIfNotNull();
            set => _IFSC = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new CheckBranchRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class CheckBranchRequestValidator : AbstractValidator<CheckBranchRequest>
    {
        public CheckBranchRequestValidator()
        {
            RuleFor(x => x.IFSC)
               .Must(new Validation().IsValidString)
               .WithMessage(BeneficiaryMessage.InvalidIFSC);
        }
    }
}