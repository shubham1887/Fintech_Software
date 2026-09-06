using FluentValidation;
namespace FintechApp.Requests.DematSetting { public class DeleteDematSettingRequest { public int DematSettingID { get; set; } } public class DeleteDematSettingRequestValidator : AbstractValidator<DeleteDematSettingRequest> { public DeleteDematSettingRequestValidator() { RuleFor(x => x.DematSettingID).GreaterThan(0); } } }
