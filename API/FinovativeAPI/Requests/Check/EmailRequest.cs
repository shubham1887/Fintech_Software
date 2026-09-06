using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Check
{
    public class EmailRequest
    {
        private string _EmailID;

        public string EmailID
        {
            get => _EmailID;
            set => _EmailID = value;
        }

        private string _Message;

        public string Message
        {
            get => _Message;
            set => _Message = value;
        }

        internal string IsValidModalState()
        {
            var validator = new EmailRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class EmailRequestValidator : AbstractValidator<EmailRequest>
    {
        public EmailRequestValidator()
        {
            RuleFor(x => x.EmailID)
                .Must(new Validation().IsValidString)
                .WithMessage(CheckMessage.InValidEmailID);

            RuleFor(x => x.Message)
               .Must(new Validation().IsValidString)
               .WithMessage(CheckMessage.InValidMessage);
        }
    }
}