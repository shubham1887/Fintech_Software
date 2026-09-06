using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.WalletTopUp
{
    public class Paysprint_GenerateOTPRequest
    {
        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber.TrimIfNotNull();
            set => _MobileNumber = value.TrimIfNotNull();
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private string _OperatorName;

        public string OperatorName
        {
            get => _OperatorName.TrimIfNotNull();
            set => _OperatorName = value.TrimIfNotNull();
        }

        private int _ProviderID;

        internal int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        private string _Pin;

        public string Pin
        {
            get => _Pin.TrimIfNotNull();
            set => _Pin = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new Paysprint_GenerateOTPRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class Paysprint_GenerateOTPRequestValidator : AbstractValidator<Paysprint_GenerateOTPRequest>
    {
        public Paysprint_GenerateOTPRequestValidator()
        {
            RuleFor(x => x.Amount)
                      .Must(new Validation().IsValidAmoutForAEPSAndmATM)
                      .WithMessage(OperatorMessage.InValidAmount);

            RuleFor(x => x.MobileNumber)
                      .Must(new Validation().IsValidMobileNumber)
                      .WithMessage(TransactionMessage.InvalidMobileNumber);
        }
    }
}