using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.MicroATM
{
    public class MorePay_ValidateOTPRequest
    {
        private string _MerchantCode;

        public string MerchantCode
        {
            get => _MerchantCode.TrimIfNotNull();
            set => _MerchantCode = value.TrimIfNotNull();
        }

        private string _OTP;

        public string OTP
        {
            get => _OTP.TrimIfNotNull();
            set => _OTP = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new ValidateOTPRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class ValidateOTPRequestValidator : AbstractValidator<MorePay_ValidateOTPRequest>
    {
        public ValidateOTPRequestValidator()
        {
            RuleFor(x => x.MerchantCode)
                      .Must(new Validation().IsValidString)
                      .WithMessage(OnboardingMessage.InvalidMerchantCode);
            RuleFor(x => x.OTP)
                     .Must(new Validation().IsValidString)
                     .WithMessage(OnboardingMessage.InvalidOTP);
        }
    }
}