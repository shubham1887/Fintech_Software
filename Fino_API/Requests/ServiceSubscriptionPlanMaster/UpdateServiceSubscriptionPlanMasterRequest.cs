using FluentValidation;
namespace FintechApp.Requests.ServiceSubscriptionPlanMaster
{
    public class UpdateServiceSubscriptionPlanMasterRequest { public int ServiceSubscriptionPlanMasterID { get; set; } public string PlanName { get; set; } = string.Empty; }
    public class UpdateServiceSubscriptionPlanMasterRequestValidator : AbstractValidator<UpdateServiceSubscriptionPlanMasterRequest> { public UpdateServiceSubscriptionPlanMasterRequestValidator() { RuleFor(x => x.ServiceSubscriptionPlanMasterID).GreaterThan(0); } }
}
