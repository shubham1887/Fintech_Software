using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.LapuTransactionDetail
{
    public class UpdateRequest
    {
        private int _LapuTransactionDetailID;

        public int LapuTransactionDetailID
        {
            get => _LapuTransactionDetailID;
            set => _LapuTransactionDetailID = value;
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
            RuleFor(x => x.LapuTransactionDetailID)
                  .Must(new Validation().IsValidInt)
                  .WithMessage(UserBalanceCappingMessages.InvalidWalletID);

            RuleFor(x => x.Amount)
                .Must(new Validation().IsValidDecimal)
                .WithMessage(UserBalanceCappingMessages.InvalidAmount);
        }
    }
}