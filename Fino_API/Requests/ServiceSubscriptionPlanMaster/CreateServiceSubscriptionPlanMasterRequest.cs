using FluentValidation;
namespace FintechApp.Requests.ServiceSubscriptionPlanMaster
{
    public class CreateServiceSubscriptionPlanMasterRequest { public string PlanName { get; set; } = string.Empty; }
    public class CreateServiceSubscriptionPlanMasterRequestValidator : AbstractValidator<CreateServiceSubscriptionPlanMasterRequest> { public CreateServiceSubscriptionPlanMasterRequestValidator() {  } }
}
