using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.CreditCardPayment
{
    public class PaysprintPayBillRequest
    {
        private string _CardNumber;

        public string CardNumber
        {
            get => _CardNumber.TrimIfNotNull();
            set => _CardNumber = value.TrimIfNotNull();
        }

        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber.TrimIfNotNull();
            set => _MobileNumber = value.TrimIfNotNull();
        }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private string _OperatorName;

        public string OperatorName
        {
            get => _OperatorName.TrimIfNotNull();
            set => _OperatorName = value.TrimIfNotNull();
        }

        private string _OTP;

        public string OTP
        {
            get => _OTP.TrimIfNotNull();
            set => _OTP = value.TrimIfNotNull();
        }

        private string _OTPRefernce;

        public string OTPRefernce
        {
            get => _OTPRefernce.TrimIfNotNull();

            set => _OTPRefernce = value.TrimIfNotNull();
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        internal string IsValidModalState()
        {
            var validator = new PaysprintPayBillRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class PaysprintPayBillRequestValidator : AbstractValidator<PaysprintPayBillRequest>
    {
        public PaysprintPayBillRequestValidator()
        {
            RuleFor(x => x.OperatorName)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.TransactionMessage.InvalidCardNetwork);

            RuleFor(x => x.MobileNumber)
            .Must(new Validation().IsValidMobileNumber)
            .WithMessage(Message.TransactionMessage.InvalidMobileNumber);

            RuleFor(x => x.Amount)
            .Must(new Validation().IsValidDecimal)
            .WithMessage(Message.OperatorMessage.InValidAmount);

            RuleFor(x => x.CardNumber)
            .Must(new Validation().IsValidCardNumber)
            .WithMessage(Message.TransactionMessage.InvalidCardNumber);

            RuleFor(x => x.OTP)
           .Must(new Validation().IsValidString)
           .WithMessage(Message.SenderMessage.InvalidOTP);

            RuleFor(x => x.OTPRefernce)
         .Must(new Validation().IsValidString)
         .WithMessage(Message.TransactionMessage.InvalidOTPRefernce);
        }
    }
}