using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.MicroATM
{
    public class InitiateImproveRequest
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

        private string _CustomerNumber;

        public string CustomerNumber
        {
            get => _CustomerNumber.TrimIfNotNull();
            set => _CustomerNumber = value.TrimIfNotNull();
        }

        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new InitiateImproveValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class InitiateImproveValidator : AbstractValidator<InitiateImproveRequest>
    {
        public InitiateImproveValidator()
        {
            RuleFor(x => x.Name)
                  .Must(new Validation().IsValidString)
                  .WithMessage(TransactionMessage.InvalidMicroATMName);

            RuleFor(x => x.Amount)
                      .Must(new Validation().IsValidAmoutForAEPSAndmATM)
                      .WithMessage(OperatorMessage.InValidAmount);

            RuleFor(x => x.OperatorID)
                      .Must(new Validation().IsValidInt)
                      .WithMessage(TransactionMessage.InvalidMATMServiceCode);
        }
    }
}