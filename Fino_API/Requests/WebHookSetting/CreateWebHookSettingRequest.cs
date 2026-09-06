using FluentValidation;
namespace FintechApp.Requests.WebHookSetting
{
    public class CreateWebHookSettingRequest { public bool IsActive { get; set; } public string Type { get; set; } = string.Empty; public string URL { get; set; } = string.Empty; public string MethodType { get; set; } = string.Empty; public string HeaderJson { get; set; } = string.Empty; }
    public class CreateWebHookSettingRequestValidator : AbstractValidator<CreateWebHookSettingRequest> { public CreateWebHookSettingRequestValidator() { RuleFor(x => x.Type).NotEmpty(); RuleFor(x => x.URL).NotEmpty(); } }
}
