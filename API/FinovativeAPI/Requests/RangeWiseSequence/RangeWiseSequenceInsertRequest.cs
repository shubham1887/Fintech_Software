using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.RangeWiseSequence
{
    public class RangeWiseSequenceInsertRequest
    {
        private int _RangeWiseSequenceID;

        public int RangeWiseSequenceID
        {
            get => _RangeWiseSequenceID;
            set => _RangeWiseSequenceID = value;
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private decimal _FromAmount;

        public decimal FromAmount
        {
            get => _FromAmount;
            set => _FromAmount = value;
        }

        private decimal _ToAmount;

        public decimal ToAmount
        {
            get => _ToAmount;
            set => _ToAmount = value;
        }

        internal string IsValidModalState()
        {
            var validator = new RangeWiseSequenceInsertRequestValidator();
            ValidationResult results = validator.Validate(this);
            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            if (!success)
            {
                return failures[0].ToString();
            }
            success = IsValidFromToAmount();
            if (!success)
                return SlabDetailMessage.ValidAmount;

            return StatusType.Ok;
        }

        private bool IsValidFromToAmount()
        {
            bool success = new Validation().IsValidFromToAmount(FromAmount, ToAmount);
            return success;
        }
    }

    public class RangeWiseSequenceInsertRequestValidator : AbstractValidator<RangeWiseSequenceInsertRequest>
    {
        public RangeWiseSequenceInsertRequestValidator()
        {
            RuleFor(x => x.ServiceID)
               .Must(new Validation().IsValidInt)
               .WithMessage(Message.ServiceMessage.Invalid);

            RuleFor(x => x.ProviderID)
              .Must(new Validation().IsValidInt)
              .WithMessage(Message.ProviderMessage.InValidProvider);

            RuleFor(x => x.OperatorID)
                 .Must(new Validation().IsValidInt)
                 .WithMessage(Message.OperatorMessage.InValidOperator);

            RuleFor(x => x.FromAmount)
               .Must(new Validation().IsValidDecimal)
               .WithMessage(Message.RangeWiseSequenceMessage.InvalidAmout);

            RuleFor(x => x.ToAmount)
               .Must(new Validation().IsValidDecimal)
               .WithMessage(Message.RangeWiseSequenceMessage.InvalidAmout);
        }
    }
}