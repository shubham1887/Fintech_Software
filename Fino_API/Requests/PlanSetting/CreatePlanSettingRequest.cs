using FluentValidation;
namespace FintechApp.Requests.PlanSetting
{
    public class CreatePlanSettingRequest { public string Name { get; set; } = string.Empty; public string UserID { get; set; } = string.Empty; public string Token { get; set; } = string.Empty; public bool IsActiveWeb { get; set; } public bool IsActive { get; set; } public string Circle { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }
    public class CreatePlanSettingRequestValidator : AbstractValidator<CreatePlanSettingRequest> { public CreatePlanSettingRequestValidator() {  } }
}
