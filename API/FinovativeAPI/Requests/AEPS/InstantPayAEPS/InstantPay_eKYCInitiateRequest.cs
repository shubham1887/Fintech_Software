using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.AEPS.InstantPayAEPS
{
    public class InstantPay_eKYCInitiateRequest
    {
        private string _Mobile;

        public string Mobile
        {
            get => _Mobile;
            set => _Mobile = value;
        }

        private string _Pan;

        public string Pan
        {
            get => _Pan.TrimIfNotNull();
            set => _Pan = value.TrimIfNotNull();
        }

        private string _Email;

        public string Email
        {
            get => _Email.TrimIfNotNull();
            set => _Email = value.TrimIfNotNull();
        }

        private string _AccountNumber;

        public string AccountNumber
        {
            get => _AccountNumber.TrimIfNotNull();
            set => _AccountNumber = value.TrimIfNotNull();
        }

        private string _IFSC;

        public string IFSC
        {
            get => _IFSC.TrimIfNotNull();
            set => _IFSC = value.TrimIfNotNull();
        }

        private string _Aadhaar;

        public string Aadhaar
        {
            get => _Aadhaar.TrimIfNotNull();
            set => _Aadhaar = value.TrimIfNotNull();
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

        private string _Consent;

        public string Consent
        {
            get => _Consent.TrimIfNotNull();
            set => _Consent = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new InstantPay_eKYCInitiateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class InstantPay_eKYCInitiateRequestValidator : AbstractValidator<InstantPay_eKYCInitiateRequest>
    {
        public InstantPay_eKYCInitiateRequestValidator()
        {
            //RuleFor(x => x.Mobile)
            //.Must(new Validation().IsValidMobileNumber)
            //.WithMessage(AEPSMessage.InvalidMobile);

           
            RuleFor(x => x.Aadhaar)
           .Must(new Validation().IsValidAadharNo)
           .WithMessage(AEPSMessage.InvalidAadhaarNumber);

            RuleFor(x => x.Latitude)
            .Must(new Validation().IsValidString)
            .WithMessage(AEPSMessage.InvalidLatitude);

            RuleFor(x => x.Longitude)
            .Must(new Validation().IsValidString)
            .WithMessage(AEPSMessage.InvalidLongitude);

        }
    }
}