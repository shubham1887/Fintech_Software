using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.eKYC
{
    public class InstantPay_eKYCInitiateRequest
    {
        private string _aadhaarNumber;

        public string aadhaarNumber
        {
            get => _aadhaarNumber.TrimIfNotNull();
            set => _aadhaarNumber = value.TrimIfNotNull();
        }
       
        private string _latitude;

        public string latitude
        {
            get => _latitude.TrimIfNotNull();
            set => _latitude = value.TrimIfNotNull();
        }

        private string _longitude;

        public string longitude
        {
            get => _longitude.TrimIfNotNull();
            set => _longitude = value.TrimIfNotNull();
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
            RuleFor(x => x.aadhaarNumber)
            .Must(new Validation().IsValidAadharNo)
            .WithMessage(Message.eKYCMessage.InvalidAadhar);
        }
    }
}
