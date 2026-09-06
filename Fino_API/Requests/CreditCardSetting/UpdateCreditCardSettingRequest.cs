using FluentValidation;
namespace FintechApp.Requests.CreditCardSetting
{
    public class UpdateCreditCardSettingRequest { public int CreditCardSettingID { get; set; } public string Name { get; set; } = string.Empty; public string MerchantID { get; set; } = string.Empty; public string Token { get; set; } = string.Empty; public string Slat { get; set; } = string.Empty; public string PublicKey { get; set; } = string.Empty; public bool IsActiveWeb { get; set; } public bool IsActive { get; set; } public decimal MinimumAmount { get; set; } public decimal MaximumAmount { get; set; } public string Mode { get; set; } = string.Empty; }
    public class UpdateCreditCardSettingRequestValidator : AbstractValidator<UpdateCreditCardSettingRequest> { public UpdateCreditCardSettingRequestValidator() { RuleFor(x => x.CreditCardSettingID).GreaterThan(0); } }
}
