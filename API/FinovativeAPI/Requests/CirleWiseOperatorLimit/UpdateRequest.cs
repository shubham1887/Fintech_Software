using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.CirleWiseOperatorLimit
{
    public class UpdateRequest
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private int _StateID;

        public int StateID
        {
            get => _StateID;
            set => _StateID = value;
        }

        private decimal _AmountLimit;

        public decimal AmountLimit
        {
            get => _AmountLimit;
            set => _AmountLimit = value;
        }

        internal string IsValidModalState()
        {
            var validator = new UpdateRequestRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class UpdateRequestRequestValidator : AbstractValidator<UpdateRequest>
    {
        public UpdateRequestRequestValidator()
        {
            RuleFor(x => x.UserID)
               .Must(new Validation().IsValidInt)
               .WithMessage(UserMessage.InvalidUser);

            RuleFor(x => x.OperatorID)
               .Must(new Validation().IsValidInt)
               .WithMessage(OperatorMessage.InValidOperator);

            RuleFor(x => x.StateID)
                .Must(new Validation().IsValidInt)
               .WithMessage(UserMessage.InvalidUser);
        }
    }
}