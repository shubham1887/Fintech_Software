using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.ROfferCommission
{
    public class SelectFRCAmountRequest
    {
        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SelectFRCAmountRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            if (!success)
            {
                return failures[0].ToString();
            }

            return StatusType.Ok;
        }
    }

    public class SelectFRCAmountRequestValidator : AbstractValidator<SelectFRCAmountRequest>
    {
        public SelectFRCAmountRequestValidator()
        {
            RuleFor(x => x.OperatorID)
             .Must(new Validation().IsValidInt)
             .WithMessage(SlabDetailMessage.InvalidOperator);
        }
    }
}