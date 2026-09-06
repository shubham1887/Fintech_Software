using FluentValidation;
namespace FintechApp.Requests.WebHookSetting
{
    public class DeleteWebHookSettingRequest { public int WebHookSettingID { get; set; } }
    public class DeleteWebHookSettingRequestValidator : AbstractValidator<DeleteWebHookSettingRequest> { public DeleteWebHookSettingRequestValidator() { RuleFor(x => x.WebHookSettingID).GreaterThan(0); } }
}
