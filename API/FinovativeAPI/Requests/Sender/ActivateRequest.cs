using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Sender
{
    public class ActivateRequest
    {
        private int _SenderID;

        public int SenderID
        {
            get => _SenderID;
            set => _SenderID = value;
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        internal string IsValidModalState()
        {
            var validator = new ActivateRequestValidator();
            ValidationResult results = validator.Validate(this);
            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class ActivateRequestValidator : AbstractValidator<ActivateRequest>
    {
        public ActivateRequestValidator()
        {
            RuleFor(x => x.SenderID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.SenderMessage.InvalidSender);
        }
    }
}