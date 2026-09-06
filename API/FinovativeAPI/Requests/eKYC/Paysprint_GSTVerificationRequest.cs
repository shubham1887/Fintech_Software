using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.eKYC
{
    public class Paysprint_GSTVerificationRequest
    {
        private string _GSTNumber;

        public string GSTNumber
        {
            get => _GSTNumber.TrimIfNotNull();
            set => _GSTNumber = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new Paysprint_GSTVerificationRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class Paysprint_GSTVerificationRequestValidator : AbstractValidator<Paysprint_GSTVerificationRequest>
    {
        public Paysprint_GSTVerificationRequestValidator()
        {
            RuleFor(x => x.GSTNumber)
           .Must(new Validation().IsValidString)
           .WithMessage(Message.eKYCMessage.InvalidGSTNumber);
        }
    }
}