using FluentValidation;
namespace FintechApp.Requests.WebHookSetting
{
    public class UpdateWebHookSettingRequest { public int WebHookSettingID { get; set; } public bool IsActive { get; set; } public string Type { get; set; } = string.Empty; public string URL { get; set; } = string.Empty; public string MethodType { get; set; } = string.Empty; public string HeaderJson { get; set; } = string.Empty; }
    public class UpdateWebHookSettingRequestValidator : AbstractValidator<UpdateWebHookSettingRequest> { public UpdateWebHookSettingRequestValidator() { RuleFor(x => x.WebHookSettingID).GreaterThan(0); } }
}
