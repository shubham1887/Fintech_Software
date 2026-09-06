using FluentValidation;
namespace FintechApp.Requests.BBPSSettings
{
    public class UpdateBBPSSettingsRequest { public int BBPSSettingID { get; set; } public string? Name { get; set; } public bool IsActiveForWebsite { get; set; } public bool IsActive { get; set; } public string? Token { get; set; } public string? AgentID { get; set; } public string? PartnerID { get; set; } public string? MerchantID { get; set; } }
    public class UpdateBBPSSettingsRequestValidator : AbstractValidator<UpdateBBPSSettingsRequest> { public UpdateBBPSSettingsRequestValidator() { RuleFor(x => x.BBPSSettingID).GreaterThan(0); } }
}
