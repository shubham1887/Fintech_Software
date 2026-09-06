using FluentValidation;
namespace FintechApp.Requests.MicroATMSetting { public class DeleteMicroATMSettingRequest { public int MicroATMSettingID { get; set; } } public class DeleteMicroATMSettingRequestValidator : AbstractValidator<DeleteMicroATMSettingRequest> { public DeleteMicroATMSettingRequestValidator() { RuleFor(x => x.MicroATMSettingID).GreaterThan(0); } } }
