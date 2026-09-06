using FluentValidation;
namespace FintechApp.Requests.OTTSetting { public class GetOTTSettingRequest { public int OTTSettingID { get; set; } } public class GetOTTSettingRequestValidator : AbstractValidator<GetOTTSettingRequest> { public GetOTTSettingRequestValidator() { RuleFor(x => x.OTTSettingID).GreaterThan(0); } } }
