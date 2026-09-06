using FluentValidation;
namespace FintechApp.Requests.AccountOpenSetting
{
    public class DeleteAccountOpenSettingRequest { public int AccountOpenSettingID { get; set; } }
    public class DeleteAccountOpenSettingRequestValidator : AbstractValidator<DeleteAccountOpenSettingRequest> { public DeleteAccountOpenSettingRequestValidator() { RuleFor(x => x.AccountOpenSettingID).GreaterThan(0); } }
}
