using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.PushNotifications
{
    public class SendPushNotificationRequest
    {
        private string _Title;

        public string Title
        {
            get => _Title.TrimIfNotNull();
            set => _Title = value.TrimIfNotNull();
        }

        private string _Description;

        public string Description
        {
            get => _Description.TrimIfNotNull();
            set => _Description = value.TrimIfNotNull();
        }

        private List<UserTypeList> _list;

        public List<UserTypeList> UserTypeList
        {
            get => _list;
            set => _list = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SendPushNotificationRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SendPushNotificationRequestValidator : AbstractValidator<SendPushNotificationRequest>
    {
        public SendPushNotificationRequestValidator()
        {
            RuleFor(x => x.Title)
                      .Must(new Validation().IsValidString)
                      .WithMessage(PushNotificationMessage.InvalidTitle);
            RuleFor(x => x.Description)
                      .Must(new Validation().IsValidString)
                      .WithMessage(PushNotificationMessage.InvalidDescription);
        }
    }
}