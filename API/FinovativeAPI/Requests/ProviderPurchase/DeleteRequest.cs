using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.ProviderPurchase
{
    public class DeleteRequest
    {
        private int _ProviderPurchaseID;

        public int ProviderPurchaseID
        {
            get => _ProviderPurchaseID;
            set => _ProviderPurchaseID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new DeleteRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class DeleteRequestValidator : AbstractValidator<DeleteRequest>
    {
        public DeleteRequestValidator()
        {
            RuleFor(x => x.ProviderPurchaseID)
                .Must(new Validation().IsValidInt)
                .WithMessage(CallMeRequestMessage.InvalidID);
        }
    }
}