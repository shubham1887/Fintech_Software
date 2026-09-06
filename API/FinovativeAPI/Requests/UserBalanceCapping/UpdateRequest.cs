using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.UserBalanceCapping
{
    public class UpdateRequest
    {
        private int _WalletID;

        public int WalletID
        {
            get => _WalletID;
            set => _WalletID = value;
        }

        private int userID;

        public int UserID
        {
            get => userID;
            set => userID = value;
        }

        private int _UserBalanceCappingID;

        public int UserBalanceCappingID
        {
            get => _UserBalanceCappingID;
            set => _UserBalanceCappingID = value;
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

    public class UpdateRequestValidator : AbstractValidator<UpdateRequest>
    {
        public UpdateRequestValidator()
        {
            RuleFor(x => x.UserBalanceCappingID)
                  .Must(new Validation().IsValidInt)
                  .WithMessage(UserBalanceCappingMessages.InvalidWalletID);

            RuleFor(x => x.Amount)
                .Must(new Validation().IsValidDecimal)
                .GreaterThanOrEqualTo(10)
                .WithMessage(UserBalanceCappingMessages.InvalidSmallerAmount);
        }
    }
}