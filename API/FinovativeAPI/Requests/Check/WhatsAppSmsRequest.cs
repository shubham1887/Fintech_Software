using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Check
{
    public class WhatsAppSmsRequest
    {
        private string _WhatsAppNumber;

        public string WhatsAppNumber
        {
            get => _WhatsAppNumber;
            set => _WhatsAppNumber = value;
        }

        private string _Message;

        public string Message
        {
            get => _Message;
            set => _Message = value;
        }

        internal string IsValidModalState()
        {
            var validator = new WhatsAppSmsRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class WhatsAppSmsRequestValidator : AbstractValidator<WhatsAppSmsRequest>
    {
        public WhatsAppSmsRequestValidator()
        {
            RuleFor(x => x.WhatsAppNumber)
                .Must(new Validation().IsValidString)
                .WithMessage(CheckMessage.InvalidMobile);

            RuleFor(x => x.Message)
               .Must(new Validation().IsValidString)
               .WithMessage(CheckMessage.InValidMessage);
        }
    }
}