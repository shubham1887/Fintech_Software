using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.UserwiseLowBalance
{
    public class UpdateRequest
    {
        private int _UserwiseLowBalanceID;

        public int UserwiseLowBalanceID
        {
            get => _UserwiseLowBalanceID;
            set => _UserwiseLowBalanceID = value;
        }

        private int _WalletID;

        public int WalletID
        {
            get => _WalletID;
            set => _WalletID = value;
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
            RuleFor(x => x.UserwiseLowBalanceID)
                .Must(new Validation().IsValidInt)
                .WithMessage(UserwiseLowBalanceMessage.InvalidID);

            RuleFor(x => x.WalletID)
              .Must(new Validation().IsValidInt)
              .WithMessage(UserwiseLowBalanceMessage.InvalidWalletID);

            RuleFor(x => x.Amount)
           .Must(new Validation().IsValidDecimal)
           .WithMessage(UserwiseLowBalanceMessage.InvalidAmount);
        }
    }
}