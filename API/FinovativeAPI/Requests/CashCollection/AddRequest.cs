using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.CashCollection
{
    public class AddRequest
    {
        private int _ReceiverID;

        public int ReceiverID
        {
            get => _ReceiverID;
            set => _ReceiverID = value;
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private string _Remark;

        public string Remark
        {
            get => _Remark.TrimIfNotNull();
            set => _Remark = value.TrimIfNotNull();
        }

        private string _PayMode;

        public string PayMode
        {
            get => _PayMode.TrimIfNotNull();
            set => _PayMode = value.TrimIfNotNull();
        }

        private string _UTRNumber;

        public string UTRNumber
        {
            get => _UTRNumber.TrimIfNotNull();
            set => _UTRNumber = value.TrimIfNotNull();
        }

        private string _ToDate;

        public string ToDate
        {
            get => _ToDate.TrimIfNotNull();
            set => _ToDate = value.TrimIfNotNull();
        }

        private string _Pin;

        public string Pin
        {
            get => _Pin.TrimIfNotNull();
            set => _Pin = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new AddRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class AddRequestValidator : AbstractValidator<AddRequest>
    {
        public AddRequestValidator()
        {
            RuleFor(x => x.ReceiverID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.UserMessage.InvalidUser);

            RuleFor(x => x.Amount)
               .Must(new Validation().IsValidDecimal)
               .WithMessage(Message.OperatorMessage.InValidAmount);

            RuleFor(x => x.Remark)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.FundMessage.InvalidRemark);
        }
    }
}