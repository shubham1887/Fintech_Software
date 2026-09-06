using FluentValidation;
namespace FintechApp.Requests.ServiceSubscriptionPlanMaster
{
    public class DeleteServiceSubscriptionPlanMasterRequest { public int ServiceSubscriptionPlanMasterID { get; set; } }
    public class DeleteServiceSubscriptionPlanMasterRequestValidator : AbstractValidator<DeleteServiceSubscriptionPlanMasterRequest> { public DeleteServiceSubscriptionPlanMasterRequestValidator() { RuleFor(x => x.ServiceSubscriptionPlanMasterID).GreaterThan(0); } }
}
