using FluentValidation;
namespace FintechApp.Requests.CashDepositSetting
{
    public class DeleteCashDepositSettingRequest { public int CashDepositSettingID { get; set; } }
    public class DeleteCashDepositSettingRequestValidator : AbstractValidator<DeleteCashDepositSettingRequest> { public DeleteCashDepositSettingRequestValidator() { RuleFor(x => x.CashDepositSettingID).GreaterThan(0); } }
}
