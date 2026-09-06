using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.eKYC
{
    public class PDRS_eKYCInitiateRequest
    {
        private string _Aadhaar;

        public string Aadhaar
        {
            get => _Aadhaar.TrimIfNotNull();
            set => _Aadhaar = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new PDRS_eKYCInitiateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class PDRS_eKYCInitiateRequestValidator : AbstractValidator<PDRS_eKYCInitiateRequest>
    {
        public PDRS_eKYCInitiateRequestValidator()
        {
            RuleFor(x => x.Aadhaar)
           .Must(new Validation().IsValidAadharNo)
           .WithMessage(Message.eKYCMessage.InvalidAadhar);
        }
    }
}