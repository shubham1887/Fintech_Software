using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Notification
{
    public class InsertRequest
    {
        private int _NotificationID;

        public int NotificationID
        {
            get => _NotificationID;
            set => _NotificationID = value;
        }

        private string _NotificationText;

        public string NotificationText
        {
            get => _NotificationText.TrimIfNotNull();
            set => _NotificationText = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new RequestValidator();
            FluentValidation.Results.ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class RequestValidator : AbstractValidator<InsertRequest>
    {
        public RequestValidator()
        {
            RuleFor(x => x.NotificationText)
              .Must(new Validation().IsValidString)
              .WithMessage(NotificationMessage.InvalidMessage);
        }
    }
}