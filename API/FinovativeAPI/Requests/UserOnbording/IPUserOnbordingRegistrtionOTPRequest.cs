using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.UserOnbording
{
    public class IPUserOnbordingRegistrtionOTPRequest
    {
        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber.TrimIfNotNull();
            set => _MobileNumber = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new IPUserOnbordingRegistrtionOTPRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class IPUserOnbordingRegistrtionOTPRequestValidator : AbstractValidator<IPUserOnbordingRegistrtionOTPRequest>
    {
        public IPUserOnbordingRegistrtionOTPRequestValidator()
        {
            RuleFor(x => x.MobileNumber)
               .Must(new Validation().IsValidString)
               .WithMessage(UserOnbordingMessage.InvalidMobile);
        }
    }
}