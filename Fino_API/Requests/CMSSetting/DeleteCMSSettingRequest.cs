using FluentValidation;
namespace FintechApp.Requests.CMSSetting { public class DeleteCMSSettingRequest { public int CMSSettingID { get; set; } } public class DeleteCMSSettingRequestValidator : AbstractValidator<DeleteCMSSettingRequest> { public DeleteCMSSettingRequestValidator() { RuleFor(x => x.CMSSettingID).GreaterThan(0); } } }
