using FluentValidation;

namespace FintechApp.Requests.Notification
{
    public class UpdateNotificationRequest
    {
        public int NotificationID { get; set; }
        public string NotificationText { get; set; } = string.Empty;
        public string NotificationImagePath { get; set; } = string.Empty;
    }

    public class UpdateNotificationRequestValidator : AbstractValidator<UpdateNotificationRequest>
    {
        public UpdateNotificationRequestValidator()
        {
            RuleFor(x => x.NotificationID).GreaterThan(0);
            RuleFor(x => x.NotificationText).NotEmpty().MaximumLength(500);
        }
    }
}
