using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.OperatorAmountWise
{
    public class InsertRequest
    {
        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private int _SerialCount;

        public int SerialCount
        {
            get => _SerialCount;
            set => _SerialCount = value;
        }

        internal string IsValidModalState()
        {
            var validator = new InsertRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class InsertRequestValidator : AbstractValidator<InsertRequest>
    {
        public InsertRequestValidator()
        {
            RuleFor(x => x.OperatorID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.OperatorMessage.InvalidOperatorID);

            RuleFor(x => x.Amount)
                .Must(new Validation().IsValidDecimal)
                .WithMessage(OperatorMessage.InvalidAmount);
        }
    }
}