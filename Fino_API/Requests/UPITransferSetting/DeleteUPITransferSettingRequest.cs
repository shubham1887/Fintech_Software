using FluentValidation;
namespace FintechApp.Requests.UPITransferSetting { public class DeleteUPITransferSettingRequest { public int UPITransferSettingID { get; set; } } public class DeleteUPITransferSettingRequestValidator : AbstractValidator<DeleteUPITransferSettingRequest> { public DeleteUPITransferSettingRequestValidator() { RuleFor(x => x.UPITransferSettingID).GreaterThan(0); } } }
