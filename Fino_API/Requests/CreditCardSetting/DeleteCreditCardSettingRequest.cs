using FluentValidation;
namespace FintechApp.Requests.CreditCardSetting
{
    public class DeleteCreditCardSettingRequest { public int CreditCardSettingID { get; set; } }
    public class DeleteCreditCardSettingRequestValidator : AbstractValidator<DeleteCreditCardSettingRequest> { public DeleteCreditCardSettingRequestValidator() { RuleFor(x => x.CreditCardSettingID).GreaterThan(0); } }
}
