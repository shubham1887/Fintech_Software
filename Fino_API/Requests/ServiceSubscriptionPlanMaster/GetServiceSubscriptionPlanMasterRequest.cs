using FluentValidation;
namespace FintechApp.Requests.ServiceSubscriptionPlanMaster
{
    public class GetServiceSubscriptionPlanMasterRequest { public int ServiceSubscriptionPlanMasterID { get; set; } }
    public class GetServiceSubscriptionPlanMasterRequestValidator : AbstractValidator<GetServiceSubscriptionPlanMasterRequest> { public GetServiceSubscriptionPlanMasterRequestValidator() { RuleFor(x => x.ServiceSubscriptionPlanMasterID).GreaterThan(0); } }
}
