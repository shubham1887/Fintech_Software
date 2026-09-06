using FluentValidation;
namespace FintechApp.Requests.SDKSetting { public class GetSDKSettingRequest { public int SDKSettingID { get; set; } } public class GetSDKSettingRequestValidator : AbstractValidator<GetSDKSettingRequest> { public GetSDKSettingRequestValidator() { RuleFor(x => x.SDKSettingID).GreaterThan(0); } } }
