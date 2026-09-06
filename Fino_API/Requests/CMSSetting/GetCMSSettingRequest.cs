using FluentValidation;
namespace FintechApp.Requests.CMSSetting { public class GetCMSSettingRequest { public int CMSSettingID { get; set; } } public class GetCMSSettingRequestValidator : AbstractValidator<GetCMSSettingRequest> { public GetCMSSettingRequestValidator() { RuleFor(x => x.CMSSettingID).GreaterThan(0); } } }
