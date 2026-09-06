using FluentValidation;
namespace FintechApp.Requests.CashDepositSetting
{
    public class UpdateCashDepositSettingRequest { public int CashDepositSettingID { get; set; } public string Name { get; set; } = string.Empty; public string MerchantID { get; set; } = string.Empty; public string Token { get; set; } = string.Empty; public string Slat { get; set; } = string.Empty; public string? PublicKey { get; set; } public bool IsActiveWeb { get; set; } public bool IsActive { get; set; } public decimal MinimumAmount { get; set; } public decimal MaximumAmount { get; set; } public string? Mode { get; set; } }
    public class UpdateCashDepositSettingRequestValidator : AbstractValidator<UpdateCashDepositSettingRequest> { public UpdateCashDepositSettingRequestValidator() { RuleFor(x => x.CashDepositSettingID).GreaterThan(0); } }
}
