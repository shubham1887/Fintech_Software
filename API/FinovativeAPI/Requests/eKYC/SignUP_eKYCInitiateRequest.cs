using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.eKYC
{
    public class SignUP_eKYCInitiateRequest
    {

        private string _Aadhaar;

        public string Aadhaar
        {
            get => _Aadhaar.TrimIfNotNull();
            set => _Aadhaar = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new SignUP_eKYCInitiateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SignUP_eKYCInitiateRequestValidator : AbstractValidator<SignUP_eKYCInitiateRequest>
    {
        public SignUP_eKYCInitiateRequestValidator()
        {
            RuleFor(x => x.Aadhaar)
           .Must(new Validation().IsValidAadharNo)
           .WithMessage(Message.eKYCMessage.InvalidAadhar);
        }
    }
}