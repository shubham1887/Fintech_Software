using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.UserCreationMaster
{
    public class TransferRequest
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private int _PurchaseCount;

        public int PurchaseCount
        {
            get => _PurchaseCount;
            set => _PurchaseCount = value;
        }

        internal string IsValidModalState()
        {
            var validator = new TransferRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class TransferRequestValidator : AbstractValidator<TransferRequest>
    {
        public TransferRequestValidator()
        {
            RuleFor(x => x.UserID)
                .Must(new Validation().IsValidInt)
                .WithMessage(UserCreationMessage.InvalidCount);

            RuleFor(x => x.PurchaseCount)
               .Must(new Validation().IsValidInt)
               .WithMessage(UserCreationMessage.InvalidCount);
        }
    }
}