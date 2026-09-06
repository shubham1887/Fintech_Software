using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.AEPS.InstantPayAEPS
{
    public class InstantPay_OutletLoginRequest
    {
        private string _adhaarnumber;

        public string adhaarnumber
        {
            get => _adhaarnumber;
            set => _adhaarnumber = value;
        }

        private string _FingerPrint;

        public string FingerPrint
        {
            get => _FingerPrint;
            set => _FingerPrint = value;
        }

        private string _Type;

        public string Type
        {
            get => _Type?.TrimIfNotNull();
            set => _Type = value?.TrimIfNotNull();
        }

        private string _IIN;

        public string IIN
        {
            get => _IIN.TrimIfNotNull();
            set => _IIN = value.TrimIfNotNull();
        }

        private string _Latitude;

        public string Latitude
        {
            get => _Latitude?.TrimIfNotNull();
            set => _Latitude = value?.TrimIfNotNull();
        }

        private string _Longitude;

        public string Longitude
        {
            get => _Longitude?.TrimIfNotNull();
            set => _Longitude = value?.TrimIfNotNull();
        }

        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new InstantPay_OutletLoginRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class InstantPay_OutletLoginRequestValidator : AbstractValidator<InstantPay_OutletLoginRequest>
    {
        public InstantPay_OutletLoginRequestValidator()
        {
            RuleFor(x => x.adhaarnumber)
               .Must(new Validation().IsValidString)
               .WithMessage(AEPSMessage.InvalidAadhaarNumber);

            RuleFor(x => x.Latitude)
                      .Must(new Validation().IsValidString)
                      .WithMessage(AEPSMessage.InvalidLatitude);

            RuleFor(x => x.Longitude)
                      .Must(new Validation().IsValidString)
                      .WithMessage(AEPSMessage.InvalidLongitude);

            RuleFor(x => x.Type)
           .Must(new Validation().IsValidString)
           .WithMessage(AEPSMessage.InvalidType);

            When(x => x.Type == "TXN_AUTH", () =>
            {
                RuleFor(x => x.IIN)
                    .NotEmpty()
                    .WithMessage("IIN cannot be empty for TXN_AUTH type.");
            });
        }
    }
}