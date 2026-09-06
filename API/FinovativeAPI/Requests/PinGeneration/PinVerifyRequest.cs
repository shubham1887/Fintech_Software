using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.Requests.User;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.PinGeneration
{
    public class PinVerifyRequest
    {

        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber;
            set => _MobileNumber = value;
        }

        private string _OTP;

        public string OTP
        {
            get => _OTP.TrimIfNotNull();
            set => _OTP = value.TrimIfNotNull();
        }

        private int _Pin;
        public int Pin
        {
            get => _Pin;
            set => _Pin = value;
        }

        internal string IsValidModalState()
        {
            var validator = new PinVerifyRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class PinVerifyRequestValidator : AbstractValidator<PinVerifyRequest>
    {
        public PinVerifyRequestValidator()
        {
            RuleFor(x => x.OTP)
              .Must(new Validation().IsValidMobileNumber)
              .WithMessage(Message.UserMessage.InvalidOTP);

            RuleFor(x => x.MobileNumber)
             .Must(new Validation().IsValidMobileNumber)
             .WithMessage(Message.UserMessage.InvalidMobileNumber);

            RuleFor(x => x.Pin)
              .Must(new Validation().IsValidInt)
              .WithMessage(Message.UserMessage.InvalidPin);
        }
    }
}