using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.AEPS
{
    public class EzyPay_InitiateRequest
    {
        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private int _Amount;

        public int Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        internal string IsValidModalState()
        {
            var validator = new EzyPay_InitiateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class EzyPay_InitiateRequestValidator : AbstractValidator<EzyPay_InitiateRequest>
    {
        public EzyPay_InitiateRequestValidator()
        {
            RuleFor(x => x.OperatorID)
              .Must(new Validation().IsValidInt)
              .WithMessage("Invalid Operator");
        }
    }
}