using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.VirtualBalance
{
    public class AddRequest
    {
        private decimal _Amount;
        private int _WalletID; public int WalletID { get => _WalletID; set => _WalletID = value; }
        public decimal Amount { get => _Amount; set => _Amount = value; }

        internal string IsValidModalState()
        {
            var validator = new AddRequestValidator();
            FluentValidation.Results.ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class AddRequestValidator : AbstractValidator<AddRequest>
    {
        public AddRequestValidator()
        {
            RuleFor(x => x.Amount)
                .Must(new Validation().IsValidDecimal)
                .WithMessage(VirtualBalanceMessage.InValidAmount);
            RuleFor(x => x.Amount)
               .Must(new Validation().IsValidVirtualBalance)
               .WithMessage(VirtualBalanceMessage.MaxValidAmount);
        }
    }
}