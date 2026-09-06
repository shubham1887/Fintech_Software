using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.PinGeneration
{
    public class ForgotMpinRequest
    {
        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber;
            set => _MobileNumber = value;
        }

        private string _PANNo;

        public string PANNo
        {
            get => _PANNo;
            set => _PANNo = value;
        }

        internal string IsValidModalState()
        {
            var validator = new ForgotMpinRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class ForgotMpinRequestValidator : AbstractValidator<ForgotMpinRequest>
    {
        public ForgotMpinRequestValidator()
        {
            RuleFor(x => x.MobileNumber)
                          .Must(new Validation().IsValidMobileNumber)
                          .WithMessage(Message.UserMessage.InvalidMobileNumber);
        }
    }
}