using FluentValidation;
namespace FintechApp.Requests.UserBalanceHistory
{
    public class CreateUserBalanceHistoryRequest { public int UserID { get; set; } public int WalletID { get; set; } public decimal Balance { get; set; } }
    public class CreateUserBalanceHistoryRequestValidator : AbstractValidator<CreateUserBalanceHistoryRequest> { public CreateUserBalanceHistoryRequestValidator() { RuleFor(x => x.UserID).GreaterThan(0); RuleFor(x => x.WalletID).GreaterThan(0); } }
}
