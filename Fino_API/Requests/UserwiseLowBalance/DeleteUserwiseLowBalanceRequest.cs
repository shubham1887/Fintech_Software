using FluentValidation;
namespace FintechApp.Requests.UserwiseLowBalance
{
    public class DeleteUserwiseLowBalanceRequest { public int UserwiseLowBalanceID { get; set; } }
    public class DeleteUserwiseLowBalanceRequestValidator : AbstractValidator<DeleteUserwiseLowBalanceRequest> { public DeleteUserwiseLowBalanceRequestValidator() { RuleFor(x => x.UserwiseLowBalanceID).GreaterThan(0); } }
}
