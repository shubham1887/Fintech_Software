using FluentValidation;
namespace FintechApp.Requests.POSSettingMaster { public class GetPOSSettingMasterRequest { public int POSSettingMasterID { get; set; } } public class GetPOSSettingMasterRequestValidator : AbstractValidator<GetPOSSettingMasterRequest> { public GetPOSSettingMasterRequestValidator() { RuleFor(x => x.POSSettingMasterID).GreaterThan(0); } } }
