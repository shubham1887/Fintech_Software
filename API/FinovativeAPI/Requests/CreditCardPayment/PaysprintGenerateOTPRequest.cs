using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.CreditCardPayment
{
    public class PaysprintGenerateOTPRequest
    {
        private string _CardNumber;

        public string CardNumber
        {
            get => _CardNumber.TrimIfNotNull();
            set => _CardNumber = value.TrimIfNotNull();
        }

        private string _Pin;

        public string Pin
        {
            get => _Pin.TrimIfNotNull();
            set => _Pin = value.TrimIfNotNull();
        }

        private int _ProviderID;

        internal int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
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

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private string _OperatorName;

        public string OperatorName
        {
            get => _OperatorName.TrimIfNotNull();
            set => _OperatorName = value.TrimIfNotNull();
        }
        private string _PayeeName;

        public string PayeeName
        {
            get => _PayeeName.TrimIfNotNull();
            set => _PayeeName = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new PaysprintGenerateOTPRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class PaysprintGenerateOTPRequestValidator : AbstractValidator<PaysprintGenerateOTPRequest>
    {
        public PaysprintGenerateOTPRequestValidator()
        {
            RuleFor(x => x.OperatorName)
          .Must(new Validation().IsValidString)
          .WithMessage(Message.TransactionMessage.InvalidCardNetwork);

            RuleFor(x => x.OperatorID)
            .Must(new Validation().IsValidInt)
            .WithMessage(Message.OperatorMessage.InvalidOperatorID);

            RuleFor(x => x.Name)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.TransactionMessage.InvalidName);

            RuleFor(x => x.MobileNumber)
            .Must(new Validation().IsValidMobileNumber)
            .WithMessage(Message.TransactionMessage.InvalidMobileNumber);

            RuleFor(x => x.Amount)
            .Must(new Validation().IsValidDecimal)
            .WithMessage(Message.OperatorMessage.InValidAmount);

            RuleFor(x => x.CardNumber)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.TransactionMessage.InvalidCardNumber);

            RuleFor(x => x.PayeeName)
           .Must(new Validation().IsValidString)
           .WithMessage(Message.TransactionMessage.InvalidPayeeName);


        }
    }
}