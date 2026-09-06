using FluentValidation;
namespace FintechApp.Requests.ROfferCommission
{
    public class DeleteROfferCommissionRequest { public int ROfferCommissionID { get; set; } }
    public class DeleteROfferCommissionRequestValidator : AbstractValidator<DeleteROfferCommissionRequest> { public DeleteROfferCommissionRequestValidator() { RuleFor(x => x.ROfferCommissionID).GreaterThan(0); } }
}
