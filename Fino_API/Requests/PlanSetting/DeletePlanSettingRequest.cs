using FluentValidation;
namespace FintechApp.Requests.PlanSetting
{
    public class DeletePlanSettingRequest { public int PlanSettingID { get; set; } }
    public class DeletePlanSettingRequestValidator : AbstractValidator<DeletePlanSettingRequest> { public DeletePlanSettingRequestValidator() { RuleFor(x => x.PlanSettingID).GreaterThan(0); } }
}
