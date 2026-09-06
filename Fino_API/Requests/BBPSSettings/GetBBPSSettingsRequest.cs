using FluentValidation;
namespace FintechApp.Requests.BBPSSettings
{
    public class GetBBPSSettingsRequest { public int BBPSSettingID { get; set; } }
    public class GetBBPSSettingsRequestValidator : AbstractValidator<GetBBPSSettingsRequest> { public GetBBPSSettingsRequestValidator() { RuleFor(x => x.BBPSSettingID).GreaterThan(0); } }
}
