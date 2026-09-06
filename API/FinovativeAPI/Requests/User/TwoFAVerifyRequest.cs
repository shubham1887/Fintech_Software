using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.User
{
    public class TwoFAVerifyRequest
    {
        private string _TwoFAPasscode;

        public string TwoFAPasscode
        {
            get => _TwoFAPasscode.TrimIfNotNull();
            set => _TwoFAPasscode = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new TwoFAVerifyRequestValidator();
            FluentValidation.Results.ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            if (!success)
            {
                return failures[0].ToString();
            }

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class TwoFAVerifyRequestValidator : AbstractValidator<TwoFAVerifyRequest>
    {
        public TwoFAVerifyRequestValidator()
        {
            RuleFor(x => x.TwoFAPasscode)
                .Must(new Validation().IsValidString)
                .WithMessage(Message.UserMessage.Invalid2FAPasscode);
        }
    }
}