using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.OTP
{
    public class SendRequest
    {
        private string _Type;

        public string Type
        {
            get => _Type.TrimIfNotNull();
            set => _Type = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new SendRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SendRequestValidator : AbstractValidator<SendRequest>
    {
        private readonly Validation validation = new Validation();

        public SendRequestValidator()
        {
            RuleFor(x => x.Type)
                .Must(validation.IsValidString)
                .WithMessage(Messages.InvalidOTPRequest);
        }
    }
}