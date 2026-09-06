using FluentValidation;
namespace FintechApp.Requests.BBPSSettings
{
    public class DeleteBBPSSettingsRequest { public int BBPSSettingID { get; set; } }
    public class DeleteBBPSSettingsRequestValidator : AbstractValidator<DeleteBBPSSettingsRequest> { public DeleteBBPSSettingsRequestValidator() { RuleFor(x => x.BBPSSettingID).GreaterThan(0); } }
}
