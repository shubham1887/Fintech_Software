using FluentValidation;
namespace FintechApp.Requests.Redeemcommission
{
    public class GetRedeemcommissionRequest { public int RedeemcommissionID { get; set; }
    public int UserID { get; set; } }
    public class GetRedeemcommissionRequestValidator : AbstractValidator<GetRedeemcommissionRequest> { public GetRedeemcommissionRequestValidator() { RuleFor(x => x.RedeemcommissionID).GreaterThan(0); } }
}
