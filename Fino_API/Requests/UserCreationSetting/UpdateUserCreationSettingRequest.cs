using FluentValidation;

namespace FintechApp.Requests.UserCreationSetting
{
    public class UpdateUserCreationSettingRequest { public int UserCreationSettingID { get; set; } public int UserID { get; set; } public decimal Cost { get; set; } public int MinimumCount { get; set; } }
    public class UpdateUserCreationSettingRequestValidator : AbstractValidator<UpdateUserCreationSettingRequest>
    {
        public UpdateUserCreationSettingRequestValidator() { RuleFor(x => x.UserCreationSettingID).GreaterThan(0); }
    }
}
