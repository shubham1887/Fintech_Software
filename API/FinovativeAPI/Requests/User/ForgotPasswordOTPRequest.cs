using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.User
{
    public class ForgotPasswordOTPRequest
    {
        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber;
            set => _MobileNumber = value;
        }

        private string _CaptchaToken;

        public string CaptchaToken
        {
            get => _CaptchaToken.TrimIfNotNull();
            set => _CaptchaToken = value.TrimIfNotNull();
        }

        private string _Device;

        public string Device
        {
            get => _Device.TrimIfNotNull();
            set => _Device = value.TrimIfNotNull();
        }

        private string _OTP;

        public string OTP
        {
            get => _OTP.TrimIfNotNull();
            set => _OTP = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new ForgotPasswordOTPRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class ForgotPasswordOTPRequestValidator : AbstractValidator<ForgotPasswordOTPRequest>
    {
        public ForgotPasswordOTPRequestValidator()
        {
            RuleFor(x => x.MobileNumber)
              .Must(new Validation().IsValidMobileNumber)
              .WithMessage(Message.UserMessage.InvalidMobileNumber);

            RuleFor(x => x.OTP).NotEmpty().WithMessage(Message.UserMessage.InvalidOTP);
        }
    }
}