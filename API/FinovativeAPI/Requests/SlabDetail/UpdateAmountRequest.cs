using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.SlabDetail
{
    public class UpdateAmountRequest
    {
        private int _SlabDetailID;

        public int SlabDetailID
        {
            get => _SlabDetailID;
            set => _SlabDetailID = value;
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
            var validator = new UpdateAmountRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            if (!success)
            {
                return failures[0].ToString();
            }
            success = IsValidFromToAmount();
            if (!success)
            {
                return SlabDetailMessage.ValidAmount;
            }

            return StatusType.Ok;
        }

        private bool IsValidFromToAmount()
        {
            bool success = new Validation().IsValidFromToAmount(FromAmount, ToAmount);
            return success;
        }
    }

    public class UpdateAmountRequestValidator : AbstractValidator<UpdateAmountRequest>
    {
        public UpdateAmountRequestValidator()
        {
            RuleFor(x => x.FromAmount)
                .Must(new Validation().IsValidDecimal)
                .WithMessage(OperatorMessage.InvalidMinimumRechargeAmount);

            RuleFor(x => x.ToAmount)
                .Must(new Validation().IsValidDecimal)
                .WithMessage(OperatorMessage.InvalidMaximumRechargeAmount);
        }
    }
}