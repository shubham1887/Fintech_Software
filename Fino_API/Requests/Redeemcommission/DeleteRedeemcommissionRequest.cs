using FluentValidation;
namespace FintechApp.Requests.Redeemcommission
{
    public class DeleteRedeemcommissionRequest { public int RedeemcommissionID { get; set; } }
    public class DeleteRedeemcommissionRequestValidator : AbstractValidator<DeleteRedeemcommissionRequest> { public DeleteRedeemcommissionRequestValidator() { RuleFor(x => x.RedeemcommissionID).GreaterThan(0); } }
}
