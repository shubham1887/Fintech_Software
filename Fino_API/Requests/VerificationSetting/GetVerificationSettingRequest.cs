using FluentValidation;
namespace FintechApp.Requests.VerificationSetting { public class GetVerificationSettingRequest { public int VerificationSettingID { get; set; } } public class GetVerificationSettingRequestValidator : AbstractValidator<GetVerificationSettingRequest> { public GetVerificationSettingRequestValidator() { RuleFor(x => x.VerificationSettingID).GreaterThan(0); } } }
