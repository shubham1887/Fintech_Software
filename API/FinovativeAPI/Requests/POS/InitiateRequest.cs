using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.POS
{
    public class InitiateRequest
    {
        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private string _ConsumerNumber;

        public string ConsumerNumber
        {
            get => _ConsumerNumber.TrimIfNotNull();
            set => _ConsumerNumber = value.TrimIfNotNull();
        }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new InitiateValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class InitiateValidator : AbstractValidator<InitiateRequest>
    {
        public InitiateValidator()
        {
            RuleFor(x => x.Name)
                  .Must(new Validation().IsValidString)
                  .WithMessage(TransactionMessage.InvalidMicroATMName);

            RuleFor(x => x.Amount)
                      .Must(new Validation().IsValidAmoutForAEPSAndmATM)
                      .WithMessage(OperatorMessage.InValidAmount);
        }
    }
}