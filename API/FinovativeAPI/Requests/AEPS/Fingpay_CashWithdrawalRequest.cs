using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.AEPS
{
    public class Fingpay_CashWithdrawalRequest
    {
        private string _AadhaarNumber;

        public string AadhaarNumber
        {
            get => _AadhaarNumber.TrimIfNotNull();
            set => _AadhaarNumber = value.TrimIfNotNull();
        }

        private string _BankName;

        public string BankName
        {
            get => _BankName.TrimIfNotNull();
            set => _BankName = value.TrimIfNotNull();
        }

        private string _ConsumerNumber;

        public string ConsumerNumber
        {
            get => _ConsumerNumber.TrimIfNotNull();
            set => _ConsumerNumber = value.TrimIfNotNull();
        }

        private string _IIN;

        public string IIN
        {
            get => _IIN.TrimIfNotNull();
            set => _IIN = value.TrimIfNotNull();
        }

        private string _FingerPrint;

        public string FingerPrint
        {
            get => _FingerPrint.TrimIfNotNull();
            set => _FingerPrint = value.TrimIfNotNull();
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private string _Through;

        internal string Through
        {
            get => _Through.TrimIfNotNullWithDefault(PlatformCode.Web);
            set => _Through = value.TrimIfNotNullWithDefault(PlatformCode.Web);
        }

        private string _APIUserRequestID;

        public string APIUserRequestID
        {
            get => _APIUserRequestID.TrimIfNotNull();
            set => _APIUserRequestID = value.TrimIfNotNull();
        }
        private string _Latitude;
        public string Latitude
        {
            get => _Latitude.TrimIfNotNull();
            set => _Latitude = value.TrimIfNotNull();
        }
        private string _Longitude;
        public string Longitude
        {
            get => _Longitude.TrimIfNotNull();
            set => _Longitude = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new Fingpay_CashWithdrawalRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class Fingpay_CashWithdrawalRequestValidator : AbstractValidator<Fingpay_CashWithdrawalRequest>
    {
        public Fingpay_CashWithdrawalRequestValidator()
        {
            RuleFor(x => x.IIN)
                      .Must(new Validation().IsValidString)
                      .WithMessage(AEPSMessage.InvalidIIN);

            RuleFor(x => x.AadhaarNumber)
                      .Must(new Validation().IsValidAadharNo)
                      .WithMessage(AEPSMessage.InvalidAadhaarNumber);

            RuleFor(x => x.FingerPrint)
                    .Must(m => new Validation().IsValidStringWithMinimumLength(m, length: 20))
                    .WithMessage(AEPSMessage.InvalidFingerPrint);

            RuleFor(x => x.Amount)
                    .Must(new Validation().IsValidAmoutForAEPSAndmATM)
                    .WithMessage(OperatorMessage.InValidAmount);
        }
    }
}