using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;
namespace Project.Requests.PanMitra
{
    public class CouponStatusCheckRequest
    {
        private int _ID;
        public int ID
        {
            get => _ID;
            set => _ID = value;
        }
        internal string IsValidModalState()
        {
            var validator = new CouponStatusCheckRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class CouponStatusCheckRequestValidator : AbstractValidator<CouponStatusCheckRequest>
    {
        public CouponStatusCheckRequestValidator()
        {
            RuleFor(x => x.ID)
             .Must(new Validation().IsValidInt)
             .WithMessage(Message.PanMessage.InvalidTransactionID);

        }
    }
}

