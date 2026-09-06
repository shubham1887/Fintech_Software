using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.UserCreationMaster
{
    public class InsertRequest
    {
        private int _WalletID;

        public int WalletID
        {
            get => _WalletID;
            set => _WalletID = value;
        }

        private int _PurchaseCount;

        public int PurchaseCount
        {
            get => _PurchaseCount;
            set => _PurchaseCount = value;
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        internal string IsValidModalState()
        {
            var validator = new InsertRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class InsertRequestValidator : AbstractValidator<InsertRequest>
    {
        public InsertRequestValidator()
        {
            RuleFor(x => x.PurchaseCount)
                .Must(new Validation().IsValidInt)
                .WithMessage(UserCreationMessage.InvalidCount);

            RuleFor(x => x.WalletID)
               .Must(new Validation().IsValidInt)
               .WithMessage(WalletMessage.Invalid);

            RuleFor(x => x.Amount)
               .Must(new Validation().IsValidDecimal)
               .WithMessage(UserCreationMessage.InvalidCost);
        }
    }
}