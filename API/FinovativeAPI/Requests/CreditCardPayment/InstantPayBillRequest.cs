using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.CreditCardPayment
{
    public class InstantPayBillRequest
    {
        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
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

        private string _CardNumber;

        public string CardNumber
        {
            get => _CardNumber.TrimIfNotNull();
            set => _CardNumber = value.TrimIfNotNull();
        }

        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private string _Pin;

        public string Pin
        {
            get => _Pin.TrimIfNotNull();
            set => _Pin = value.TrimIfNotNull();
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

        internal string IsValidModalState()
        {
            var validator = new InstantPayBillRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class InstantPayBillRequestValidator : AbstractValidator<InstantPayBillRequest>
    {
        public InstantPayBillRequestValidator()
        {
            RuleFor(x => x.MobileNumber)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.TransactionMessage.InvalidMobileNumber);

            RuleFor(x => x.Amount)
            .Must(new Validation().IsValidDecimal)
            .WithMessage(Message.OperatorMessage.InValidAmount);

            RuleFor(x => x.CardNumber)
            .Must(new Validation().IsValidCardNumber)
            .WithMessage(Message.TransactionMessage.InvalidCardNumber);

            RuleFor(x => x.Latitude)
             .Must(new Validation().IsValidString)
             .WithMessage(AEPSMessage.InvalidLatitude);

            RuleFor(x => x.Longitude)
             .Must(new Validation().IsValidString)
             .WithMessage(AEPSMessage.InvalidLongitude);
        }
    }
}