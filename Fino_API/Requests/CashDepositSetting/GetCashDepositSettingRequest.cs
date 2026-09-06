using FluentValidation;
namespace FintechApp.Requests.CashDepositSetting
{
    public class GetCashDepositSettingRequest { public int CashDepositSettingID { get; set; } }
    public class GetCashDepositSettingRequestValidator : AbstractValidator<GetCashDepositSettingRequest> { public GetCashDepositSettingRequestValidator() { RuleFor(x => x.CashDepositSettingID).GreaterThan(0); } }
}
