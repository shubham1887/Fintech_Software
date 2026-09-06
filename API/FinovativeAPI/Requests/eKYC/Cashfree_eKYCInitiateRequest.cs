using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;


namespace Project.Requests.eKYC
{
    public class Cashfree_eKYCInitiateRequest
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
            var validator = new Cashfree_eKYCInitiateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class Cashfree_eKYCInitiateRequestValidator : AbstractValidator<Cashfree_eKYCInitiateRequest>
    {
        public Cashfree_eKYCInitiateRequestValidator()
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