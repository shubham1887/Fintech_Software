using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.VirtualBalance
{
    public class AddWithOTPRequest
    {
        private decimal _Amount;
        private int _WalletID;
        public decimal Amount { get => _Amount; set => _Amount = value; }
        public string OTP { get => _OTP.TrimIfNotNull(); set => _OTP = value.TrimIfNotNull(); }
        public int WalletID { get => _WalletID; set => _WalletID = value; }

        private string _OTP;

        internal string IsValidModalState()
        {
            var validator = new AddWithOTPRequestValidator();
            FluentValidation.Results.ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class AddWithOTPRequestValidator : AbstractValidator<AddWithOTPRequest>
    {
        public AddWithOTPRequestValidator()
        {
            RuleFor(x => x.Amount)
                .Must(new Validation().IsValidDecimal)
                .WithMessage(OperatorMessage.InValidAmount);

            RuleFor(x => x.OTP)
               .Must(new Validation().IsValidString)
               .WithMessage(UserMessage.InvalidOTP);
        }
    }
}