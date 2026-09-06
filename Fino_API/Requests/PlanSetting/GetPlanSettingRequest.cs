using FluentValidation;
namespace FintechApp.Requests.PlanSetting
{
    public class GetPlanSettingRequest { public int PlanSettingID { get; set; } }
    public class GetPlanSettingRequestValidator : AbstractValidator<GetPlanSettingRequest> { public GetPlanSettingRequestValidator() { RuleFor(x => x.PlanSettingID).GreaterThan(0); } }
}
