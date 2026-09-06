using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.SlabDetail
{
    public class SelectAmountRequest
    {
        private int _Amount;

        public int Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SelectAmountRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return (!success) ? failures[0].ToString() : StatusType.Ok;
        }
    }

    public class SelectAmountRequestValidator : AbstractValidator<SelectAmountRequest>
    {
        public SelectAmountRequestValidator()
        {
            RuleFor(x => x.Amount)
                .Must(new Validation().IsValidInt)
                .WithMessage(SlabDetailMessage.InvalidAmout);
        }
    }
}