using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.eKYC
{
    public class Quick_eKYCInitiateRequest
    {
        private string _Pan;

        public string Pan
        {
            get => _Pan.TrimIfNotNull();
            set => _Pan = value.TrimIfNotNull();
        }

        private string _Aadhaar;

        public string Aadhaar
        {
            get => _Aadhaar.TrimIfNotNull();
            set => _Aadhaar = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new Quick_eKYCInitiateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class Quick_eKYCInitiateRequestValidator : AbstractValidator<Quick_eKYCInitiateRequest>
    {
        public Quick_eKYCInitiateRequestValidator()
        {
            RuleFor(x => x.Pan)
            .Must(new Validation().IsValidPanCard)
            .WithMessage(Message.eKYCMessage.InvalidPAN);

            RuleFor(x => x.Aadhaar)
           .Must(new Validation().IsValidAadharNo)
           .WithMessage(Message.eKYCMessage.InvalidAadhar);
        }
    }
}