using FluentValidation;
namespace FintechApp.Requests.SDKSetting { public class DeleteSDKSettingRequest { public int SDKSettingID { get; set; } } public class DeleteSDKSettingRequestValidator : AbstractValidator<DeleteSDKSettingRequest> { public DeleteSDKSettingRequestValidator() { RuleFor(x => x.SDKSettingID).GreaterThan(0); } } }
