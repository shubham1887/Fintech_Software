using FluentValidation;
namespace FintechApp.Requests.PlanSetting
{
    public class UpdatePlanSettingRequest { public int PlanSettingID { get; set; } public string Name { get; set; } = string.Empty; public string UserID { get; set; } = string.Empty; public string Token { get; set; } = string.Empty; public bool IsActiveWeb { get; set; } public bool IsActive { get; set; } public string Circle { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }
    public class UpdatePlanSettingRequestValidator : AbstractValidator<UpdatePlanSettingRequest> { public UpdatePlanSettingRequestValidator() { RuleFor(x => x.PlanSettingID).GreaterThan(0); } }
}
