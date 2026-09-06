using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.ProviderPurchase
{
    public class UpdateRequest
    {
        private int _ProviderPurchaseID;

        public int ProviderPurchaseID
        {
            get => _ProviderPurchaseID;
            set => _ProviderPurchaseID = value;
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
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

    internal class UpdateRequestValidator : AbstractValidator<UpdateRequest>
    {
        public UpdateRequestValidator()
        {
            RuleFor(x => x.ProviderPurchaseID)
                .Must(new Validation().IsValidInt)
                .WithMessage(CallMeRequestMessage.InvalidID);
            RuleFor(x => x.Amount)
              .Must(new Validation().IsValidDecimal)
              .WithMessage(ProviderPurchaseMessage.InvalidAmount);
        }
    }
}