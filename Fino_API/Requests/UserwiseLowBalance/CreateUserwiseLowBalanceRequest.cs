using FluentValidation;
namespace FintechApp.Requests.UserwiseLowBalance
{
    public class CreateUserwiseLowBalanceRequest { public int UserID { get; set; } public int WalletID { get; set; } public decimal Amount { get; set; } }
    public class CreateUserwiseLowBalanceRequestValidator : AbstractValidator<CreateUserwiseLowBalanceRequest> { public CreateUserwiseLowBalanceRequestValidator() { RuleFor(x => x.UserID).GreaterThan(0); RuleFor(x => x.WalletID).GreaterThan(0); } }
}
