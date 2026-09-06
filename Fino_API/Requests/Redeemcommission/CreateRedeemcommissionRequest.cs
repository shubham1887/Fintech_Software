using FluentValidation;
namespace FintechApp.Requests.Redeemcommission
{
    public class CreateRedeemcommissionRequest { public int UserID { get; set; } public int WalletID { get; set; } public int Month { get; set; } public int Year { get; set; } public decimal Amount { get; set; } public string AdminRemark { get; set; } = string.Empty; public string Status { get; set; } = string.Empty; public string LastYearMonth { get; set; } = string.Empty; }
    public class CreateRedeemcommissionRequestValidator : AbstractValidator<CreateRedeemcommissionRequest> { public CreateRedeemcommissionRequestValidator() { RuleFor(x => x.UserID).GreaterThan(0); } }
}
