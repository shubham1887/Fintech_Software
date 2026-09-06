using FluentValidation;
namespace FintechApp.Requests.UserBalanceHistory
{
    public class DeleteUserBalanceHistoryRequest { public int UserBalanceHistoryID { get; set; } }
    public class DeleteUserBalanceHistoryRequestValidator : AbstractValidator<DeleteUserBalanceHistoryRequest> { public DeleteUserBalanceHistoryRequestValidator() { RuleFor(x => x.UserBalanceHistoryID).GreaterThan(0); } }
}
