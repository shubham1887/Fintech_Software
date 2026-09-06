using FluentValidation;

namespace FintechApp.Requests.UserCreationSetting
{
    public class CreateUserCreationSettingRequest { public int UserID { get; set; } public decimal Cost { get; set; } public int MinimumCount { get; set; } }
    public class CreateUserCreationSettingRequestValidator : AbstractValidator<CreateUserCreationSettingRequest>
    {
        public CreateUserCreationSettingRequestValidator() { RuleFor(x => x.UserID).GreaterThan(0); }
    }
}
