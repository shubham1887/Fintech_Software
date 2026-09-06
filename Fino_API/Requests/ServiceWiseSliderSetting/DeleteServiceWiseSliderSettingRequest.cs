using FluentValidation;
namespace FintechApp.Requests.ServiceWiseSliderSetting
{
    public class DeleteServiceWiseSliderSettingRequest { public int ServiceWiseSliderSettingID { get; set; } }
    public class DeleteServiceWiseSliderSettingRequestValidator : AbstractValidator<DeleteServiceWiseSliderSettingRequest> { public DeleteServiceWiseSliderSettingRequestValidator() { RuleFor(x => x.ServiceWiseSliderSettingID).GreaterThan(0); } }
}
