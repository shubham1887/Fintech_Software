using FluentValidation;
namespace FintechApp.Requests.ServiceWiseSliderSetting
{
    public class GetServiceWiseSliderSettingRequest { public int ServiceWiseSliderSettingID { get; set; }
    public int ServiceID { get; set; } }
    public class GetServiceWiseSliderSettingRequestValidator : AbstractValidator<GetServiceWiseSliderSettingRequest> { public GetServiceWiseSliderSettingRequestValidator() { RuleFor(x => x.ServiceWiseSliderSettingID).GreaterThan(0); } }
}
