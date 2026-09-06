using FluentValidation;

namespace FintechApp.Requests.UserCreationSetting
{
    public class GetUserCreationSettingRequest { public int UserCreationSettingID { get; set; } public int UserID { get; set; } }
    public class GetUserCreationSettingRequestValidator : AbstractValidator<GetUserCreationSettingRequest>
    {
        public GetUserCreationSettingRequestValidator() { RuleFor(x => x.UserCreationSettingID).GreaterThan(0); }
    }
}
