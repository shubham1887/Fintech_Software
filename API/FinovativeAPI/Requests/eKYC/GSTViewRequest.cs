using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;


namespace Project.Requests.eKYC
{
    public class GSTViewRequest
    {
        private string _GSTNumber;

        public string GSTNumber
        {
            get => _GSTNumber.TrimIfNotNull();
            set => _GSTNumber = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new GSTViewRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class GSTViewRequestValidator : AbstractValidator<GSTViewRequest>
    {
        public GSTViewRequestValidator()
        {
            RuleFor(x => x.GSTNumber)
           .Must(new Validation().IsValidString)
           .WithMessage(Message.eKYCMessage.InvalidGSTNumber);
        }
    }
}