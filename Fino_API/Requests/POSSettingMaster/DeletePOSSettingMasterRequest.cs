using FluentValidation;
namespace FintechApp.Requests.POSSettingMaster { public class DeletePOSSettingMasterRequest { public int POSSettingMasterID { get; set; } } public class DeletePOSSettingMasterRequestValidator : AbstractValidator<DeletePOSSettingMasterRequest> { public DeletePOSSettingMasterRequestValidator() { RuleFor(x => x.POSSettingMasterID).GreaterThan(0); } } }
