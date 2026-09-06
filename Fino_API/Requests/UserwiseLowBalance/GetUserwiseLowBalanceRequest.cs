using FluentValidation;
namespace FintechApp.Requests.UserwiseLowBalance
{
    public class GetUserwiseLowBalanceRequest { public int UserwiseLowBalanceID { get; set; } public int UserID { get; set; } }
    public class GetUserwiseLowBalanceRequestValidator : AbstractValidator<GetUserwiseLowBalanceRequest> { public GetUserwiseLowBalanceRequestValidator() { RuleFor(x => x.UserwiseLowBalanceID).GreaterThan(0); } }
}
