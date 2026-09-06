using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.MicroATM
{
    public class MorePay_SendEYCOTPRequest
    {
        private string _MerchantCode;

        public string MerchantCode
        {
            get => _MerchantCode.TrimIfNotNull();
            set => _MerchantCode = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new SendEYCOTPRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SendEYCOTPRequestValidator : AbstractValidator<MorePay_SendEYCOTPRequest>
    {
        public SendEYCOTPRequestValidator()
        {
            RuleFor(x => x.MerchantCode)
                      .Must(new Validation().IsValidString)
                      .WithMessage(OnboardingMessage.InvalidMerchantCode);
        }
    }
}