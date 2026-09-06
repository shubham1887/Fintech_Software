using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.RetailerToAllFundTransfer
{
    public class OTPRequest
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

        internal string IsValidModalState()
        {
            var validator = new OTPRequestRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class OTPRequestRequestValidator : AbstractValidator<OTPRequest>
    {
        public OTPRequestRequestValidator()
        {
            RuleFor(x => x.MobileNumber)
              .Must(new Validation().IsValidMobileNumber)
              .WithMessage(Message.UserMessage.InvalidMobileNumber);
        }
    }
}