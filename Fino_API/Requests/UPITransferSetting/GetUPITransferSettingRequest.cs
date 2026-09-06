using FluentValidation;
namespace FintechApp.Requests.UPITransferSetting { public class GetUPITransferSettingRequest { public int UPITransferSettingID { get; set; } } public class GetUPITransferSettingRequestValidator : AbstractValidator<GetUPITransferSettingRequest> { public GetUPITransferSettingRequestValidator() { RuleFor(x => x.UPITransferSettingID).GreaterThan(0); } } }
