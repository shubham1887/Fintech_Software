using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.User
{
    public class ForgotPasswordRequest
    {
        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber;
            set => _MobileNumber = value;
        }

        private string _AadharNo;

        public string AadharNo
        {
            get => _AadharNo;
            set => _AadharNo = value;
        }

        private string _PANNo;

        public string PANNo
        {
            get => _PANNo;
            set => _PANNo = value;
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

        internal string IsValidModalState()
        {
            var validator = new ForgotPasswordRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
    {
        public ForgotPasswordRequestValidator()
        {
            RuleFor(x => x.MobileNumber)
              .Must(new Validation().IsValidMobileNumber)
              .WithMessage(Message.UserMessage.InvalidMobileNumber);
        }
    }
}