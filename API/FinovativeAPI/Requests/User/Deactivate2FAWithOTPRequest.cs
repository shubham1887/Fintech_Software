using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.User
{
    public class Deactivate2FAWithOTPRequest
    {
        public string OTP { get => _OTP.TrimIfNotNull(); set => _OTP = value.TrimIfNotNull(); }

        private string _OTP;

        internal string IsValidModalState()
        {
            var validator = new Deactivate2FAWithOTPRequestValidator();
            FluentValidation.Results.ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class Deactivate2FAWithOTPRequestValidator : AbstractValidator<Deactivate2FAWithOTPRequest>
    {
        public Deactivate2FAWithOTPRequestValidator()
        {
            RuleFor(x => x.OTP)
               .Must(new Validation().IsValidString)
               .WithMessage(UserMessage.InvalidOTP);
        }
    }
}