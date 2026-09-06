using FintechApp.Messages;
using FluentValidation;

namespace FintechApp.Requests.LoginHistory
{
    public class CreateLoginHistoryRequest
    {
        public int UserID { get; set; }
        public string IPAddress { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Through { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string LoginMessage { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string SimCardDetails { get; set; } = string.Empty;
    }

    public class CreateLoginHistoryRequestValidator : AbstractValidator<CreateLoginHistoryRequest>
    {
        public CreateLoginHistoryRequestValidator()
        {
            RuleFor(x => x.UserID)
                .GreaterThan(0).WithMessage(LoginHistoryMessages.InvalidUserID);

            RuleFor(x => x.IPAddress)
                .NotEmpty().WithMessage(LoginHistoryMessages.RequiredField)
                .MaximumLength(150).WithMessage(LoginHistoryMessages.InvalidLength);

            RuleFor(x => x.Token)
                .NotEmpty().WithMessage(LoginHistoryMessages.RequiredField);

            RuleFor(x => x.Through)
                .NotEmpty().WithMessage(LoginHistoryMessages.RequiredField)
                .MaximumLength(50).WithMessage(LoginHistoryMessages.InvalidLength);

            RuleFor(x => x.LoginMessage)
                .NotEmpty().WithMessage(LoginHistoryMessages.RequiredField);
        }
    }
}
