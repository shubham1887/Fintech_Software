using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.OperatorAmountWise
{
    public class UpdateRequest
    {
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

        private int _OperatorAmountWiseSettingID;

        public int OperatorAmountWiseSettingID
        {
            get => _OperatorAmountWiseSettingID;
            set => _OperatorAmountWiseSettingID = value;
        }

        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new UpdateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class UpdateRequestValidator : AbstractValidator<UpdateRequest>
    {
        public UpdateRequestValidator()
        {
            RuleFor(x => x.OperatorAmountWiseSettingID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.OperatorMessage.InvalidID);

            RuleFor(x => x.Amount)
                .Must(new Validation().IsValidDecimal)
                .WithMessage(OperatorMessage.InvalidAmount);

            RuleFor(x => x.SerialCount)
                .Must(new Validation().IsValidInt)
                .WithMessage(OperatorMessage.InvalidDailyCount);
        }
    }
}