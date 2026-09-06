using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.SlabDetail
{
    public class ChargeAndCommissionRequest
    {
        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }
        internal string IsValidModalState()
        {
            var validator = new OperatorDeleteRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class OperatorDeleteRequestValidator : AbstractValidator<ChargeAndCommissionRequest>
    {
        public OperatorDeleteRequestValidator()
        {
            RuleFor(x => x.OperatorID)
                .Must(new Validation().IsValidInt)
                .WithMessage(OperatorMessage.InvalidOperator);
        }
    }
}
