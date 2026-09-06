using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.User
{
    public class SignUpOtpRequest
    {
        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber;
            set => _MobileNumber = value;
        }
        

        private string _Device;

        public string Device
        {
            get => _Device.TrimIfNotNull();
            set => _Device = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new SignUpOtpRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class SignUpOtpRequestValidator : AbstractValidator<SignUpOtpRequest>
    {
        public SignUpOtpRequestValidator()
        {
            RuleFor(x => x.MobileNumber)
              .Must(new Validation().IsValidMobileNumber)
              .WithMessage(Message.UserMessage.InvalidMobileNumber);
        }
    }
}