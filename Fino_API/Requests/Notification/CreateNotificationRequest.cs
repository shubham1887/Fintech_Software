using FluentValidation;

namespace FintechApp.Requests.Notification
{
    public class CreateNotificationRequest
    {
        public string NotificationText { get; set; } = string.Empty;
        public string NotificationImagePath { get; set; } = string.Empty;
    }

    public class CreateNotificationRequestValidator : AbstractValidator<CreateNotificationRequest>
    {
        public CreateNotificationRequestValidator()
        {
            RuleFor(x => x.NotificationText).NotEmpty().MaximumLength(500);
        }
    }
}
