using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.eKYC
{
    public class eKYCInitiateRequest
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

        private string _Gender;

        public string Gender
        {
            get => _Gender.TrimIfNotNull();
            set => _Gender = value.TrimIfNotNull();
        }

        private string _DOB;

        public string DOB
        {
            get => _DOB.TrimIfNotNull();
            set => _DOB = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new eKYCInitiateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class eKYCInitiateRequestValidator : AbstractValidator<eKYCInitiateRequest>
    {
        public eKYCInitiateRequestValidator()
        {
            RuleFor(x => x.Mobile)
            .Must(new Validation().IsValidMobileNumber)
            .WithMessage(Message.eKYCMessage.InvalidMobile);

            RuleFor(x => x.Pan)
            .Must(new Validation().IsValidPanCard)
            .WithMessage(Message.eKYCMessage.InvalidPAN);

            RuleFor(x => x.Aadhaar)
           .Must(new Validation().IsValidAadharNo)
           .WithMessage(Message.eKYCMessage.InvalidAadhar);

            RuleFor(x => x.Email)
          .Must(new Validation().IsValidEmail)
          .WithMessage(Message.eKYCMessage.InvalidEMail);
        }
    }
}