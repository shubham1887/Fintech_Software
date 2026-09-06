using FluentValidation;
namespace FintechApp.Requests.Redeemcommission
{
    public class UpdateRedeemcommissionRequest { public int RedeemcommissionID { get; set; } public int UserID { get; set; } public int WalletID { get; set; } public int Month { get; set; } public int Year { get; set; } public decimal Amount { get; set; } public string AdminRemark { get; set; } = string.Empty; public string Status { get; set; } = string.Empty; public string LastYearMonth { get; set; } = string.Empty; }
    public class UpdateRedeemcommissionRequestValidator : AbstractValidator<UpdateRedeemcommissionRequest> { public UpdateRedeemcommissionRequestValidator() { RuleFor(x => x.RedeemcommissionID).GreaterThan(0); } }
}
