using FluentValidation;
namespace FintechApp.Requests.UserBalanceHistory
{
    public class GetUserBalanceHistoryRequest { public int UserBalanceHistoryID { get; set; } public int UserID { get; set; } }
    public class GetUserBalanceHistoryRequestValidator : AbstractValidator<GetUserBalanceHistoryRequest> { public GetUserBalanceHistoryRequestValidator() { RuleFor(x => x.UserBalanceHistoryID).GreaterThan(0); } }
}
