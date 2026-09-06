using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Check
{
    public class SmsRequest
    {
        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber;
            set => _MobileNumber = value;
        }

        private string _Message;

        public string Message
        {
            get => _Message;
            set => _Message = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SmsRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class SmsRequestValidator : AbstractValidator<SmsRequest>
    {
        public SmsRequestValidator()
        {
            RuleFor(x => x.MobileNumber)
                .Must(new Validation().IsValidString)
                .WithMessage(CheckMessage.InvalidMobile);

            RuleFor(x => x.Message)
               .Must(new Validation().IsValidString)
               .WithMessage(CheckMessage.InValidMessage);
        }
    }
}