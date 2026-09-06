using FluentValidation;

namespace FintechApp.Requests.SettingMaster
{
    public class CreateSettingMasterRequest
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public bool IsOn { get; set; }
        public bool IsSecure { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsPassword { get; set; }
    }

    public class CreateSettingMasterRequestValidator : AbstractValidator<CreateSettingMasterRequest>
    {
        public CreateSettingMasterRequestValidator()
        {
            RuleFor(x => x.Key).NotEmpty();
            RuleFor(x => x.Value).NotEmpty();
        }
    }
}
