using FluentValidation;

namespace FintechApp.Requests.UserCreationSetting
{
    public class DeleteUserCreationSettingRequest { public int UserCreationSettingID { get; set; } }
    public class DeleteUserCreationSettingRequestValidator : AbstractValidator<DeleteUserCreationSettingRequest>
    {
        public DeleteUserCreationSettingRequestValidator() { RuleFor(x => x.UserCreationSettingID).GreaterThan(0); }
    }
}
