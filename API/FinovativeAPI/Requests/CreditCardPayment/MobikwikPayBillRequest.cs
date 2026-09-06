using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.CreditCardPayment
{
    public class MobikwikPayBillRequest
    {
        private string _CardNumber;

        public string CardNumber
        {
            get => _CardNumber.TrimIfNotNull();
            set => _CardNumber = value.TrimIfNotNull();
        }

        private string _OpCode;

        public string OpCode
        {
            get => _OpCode.TrimIfNotNull();
            set => _OpCode = value.TrimIfNotNull();
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

        internal string IsValidModalState()
        {
            var validator = new MobikwikPayBillRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class MobikwikPayBillRequestValidator : AbstractValidator<MobikwikPayBillRequest>
    {
        public MobikwikPayBillRequestValidator()
        {
            RuleFor(x => x.OperatorID)
          .Must(new Validation().IsValidInt)
          .WithMessage(Message.OperatorMessage.InvalidOperatorID);

            RuleFor(x => x.OperatorName)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.TransactionMessage.InvalidCardNetwork);

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
        }
    }
}