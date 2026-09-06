using FluentValidation;
namespace FintechApp.Requests.VerificationSetting { public class DeleteVerificationSettingRequest { public int VerificationSettingID { get; set; } } public class DeleteVerificationSettingRequestValidator : AbstractValidator<DeleteVerificationSettingRequest> { public DeleteVerificationSettingRequestValidator() { RuleFor(x => x.VerificationSettingID).GreaterThan(0); } } }
