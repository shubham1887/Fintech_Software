using FluentValidation;

namespace FintechApp.Requests.SettingMaster
{
    public class UpdateSettingMasterRequest
    {
        public int SettingMasterID { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public bool IsOn { get; set; }
        public bool IsSecure { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsPassword { get; set; }
    }

    public class UpdateSettingMasterRequestValidator : AbstractValidator<UpdateSettingMasterRequest>
    {
        public UpdateSettingMasterRequestValidator()
        {
            RuleFor(x => x.SettingMasterID).GreaterThan(0);
            RuleFor(x => x.Key).NotEmpty();
            RuleFor(x => x.Value).NotEmpty();
        }
    }
}
