using FluentValidation;
namespace FintechApp.Requests.CreditCardSetting
{
    public class GetCreditCardSettingRequest { public int CreditCardSettingID { get; set; } }
    public class GetCreditCardSettingRequestValidator : AbstractValidator<GetCreditCardSettingRequest> { public GetCreditCardSettingRequestValidator() { RuleFor(x => x.CreditCardSettingID).GreaterThan(0); } }
}
