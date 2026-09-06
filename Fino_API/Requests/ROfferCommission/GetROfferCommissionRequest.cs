using FluentValidation;
namespace FintechApp.Requests.ROfferCommission
{
    public class GetROfferCommissionRequest { public int ROfferCommissionID { get; set; }
    public int OperatorID { get; set; } }
    public class GetROfferCommissionRequestValidator : AbstractValidator<GetROfferCommissionRequest> { public GetROfferCommissionRequestValidator() { RuleFor(x => x.ROfferCommissionID).GreaterThan(0); } }
}
