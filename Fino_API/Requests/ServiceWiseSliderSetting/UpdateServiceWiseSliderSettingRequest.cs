using FluentValidation;
namespace FintechApp.Requests.ServiceWiseSliderSetting
{
    public class UpdateServiceWiseSliderSettingRequest { public int ServiceWiseSliderSettingID { get; set; } public int ServiceID { get; set; } public string ImagePath { get; set; } = string.Empty; public string Description { get; set; } = string.Empty; public string ReferenceLink { get; set; } = string.Empty; public int Priority { get; set; } public bool IsActive { get; set; } public bool IsDelete { get; set; } public string Type { get; set; } = string.Empty; }
    public class UpdateServiceWiseSliderSettingRequestValidator : AbstractValidator<UpdateServiceWiseSliderSettingRequest> { public UpdateServiceWiseSliderSettingRequestValidator() { RuleFor(x => x.ServiceWiseSliderSettingID).GreaterThan(0); } }
}
