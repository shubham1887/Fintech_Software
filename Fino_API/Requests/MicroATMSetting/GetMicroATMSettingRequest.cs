using FluentValidation;
namespace FintechApp.Requests.MicroATMSetting { public class GetMicroATMSettingRequest { public int MicroATMSettingID { get; set; } } public class GetMicroATMSettingRequestValidator : AbstractValidator<GetMicroATMSettingRequest> { public GetMicroATMSettingRequestValidator() { RuleFor(x => x.MicroATMSettingID).GreaterThan(0); } } }
