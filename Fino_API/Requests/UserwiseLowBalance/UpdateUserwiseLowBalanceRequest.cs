using FluentValidation;
namespace FintechApp.Requests.UserwiseLowBalance
{
    public class UpdateUserwiseLowBalanceRequest { public int UserwiseLowBalanceID { get; set; } public int UserID { get; set; } public int WalletID { get; set; } public decimal Amount { get; set; } }
    public class UpdateUserwiseLowBalanceRequestValidator : AbstractValidator<UpdateUserwiseLowBalanceRequest> { public UpdateUserwiseLowBalanceRequestValidator() { RuleFor(x => x.UserwiseLowBalanceID).GreaterThan(0); } }
}
