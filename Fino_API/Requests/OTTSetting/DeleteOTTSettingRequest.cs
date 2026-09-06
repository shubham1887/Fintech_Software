using FluentValidation;
namespace FintechApp.Requests.OTTSetting { public class DeleteOTTSettingRequest { public int OTTSettingID { get; set; } } public class DeleteOTTSettingRequestValidator : AbstractValidator<DeleteOTTSettingRequest> { public DeleteOTTSettingRequestValidator() { RuleFor(x => x.OTTSettingID).GreaterThan(0); } } }
