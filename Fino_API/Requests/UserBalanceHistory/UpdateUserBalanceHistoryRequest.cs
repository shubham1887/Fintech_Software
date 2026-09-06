using FluentValidation;
namespace FintechApp.Requests.UserBalanceHistory
{
    public class UpdateUserBalanceHistoryRequest { public int UserBalanceHistoryID { get; set; } public int UserID { get; set; } public int WalletID { get; set; } public decimal Balance { get; set; } }
    public class UpdateUserBalanceHistoryRequestValidator : AbstractValidator<UpdateUserBalanceHistoryRequest> { public UpdateUserBalanceHistoryRequestValidator() { RuleFor(x => x.UserBalanceHistoryID).GreaterThan(0); } }
}
