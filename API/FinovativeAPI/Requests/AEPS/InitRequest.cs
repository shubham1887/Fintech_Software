using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.AEPS
{
    public class InitRequest
    {
        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private string _Type;

        public string Type
        {
            get => _Type.TrimIfNotNull();
            set => _Type = value.TrimIfNotNull();
        }

        private string _APIUserRequestID;

        public string APIUserRequestID
        {
            get => _APIUserRequestID.TrimIfNotNull();
            set => _APIUserRequestID = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new InitRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class InitRequestValidator : AbstractValidator<InitRequest>
    {
        public InitRequestValidator()
        {
            RuleFor(x => x.Amount)
                      .Must(new Validation().IsValidAmoutForAEPSAndmATM)
                      .WithMessage(OperatorMessage.InValidAmount);

            RuleFor(x => x.Type)
                      .Must(new Validation().IsValidAEPSTypeString)
                      .WithMessage(TransactionMessage.InvalidType);
        }
    }
}