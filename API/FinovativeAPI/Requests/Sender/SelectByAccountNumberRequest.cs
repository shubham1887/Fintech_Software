using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Sender
{
    public class SelectByAccountNumberRequest
    {
        private string _AccountNumber;

        public string AccountNumber
        {
            get => _AccountNumber.TrimIfNotNull();
            set => _AccountNumber = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new SelectByAccountNumberRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectByAccountNumberRequestValidator : AbstractValidator<SelectByAccountNumberRequest>
    {
        public SelectByAccountNumberRequestValidator()
        {
            RuleFor(x => x.AccountNumber)
               .Must(new Validation().IsValidString)
               .WithMessage(Message.BeneficiaryMessage.InvalidAccountNo);
        }
    }
}