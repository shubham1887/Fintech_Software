using FluentValidation;
namespace FintechApp.Requests.WebHookSetting
{
    public class GetWebHookSettingRequest { public int WebHookSettingID { get; set; } }
    public class GetWebHookSettingRequestValidator : AbstractValidator<GetWebHookSettingRequest> { public GetWebHookSettingRequestValidator() { RuleFor(x => x.WebHookSettingID).GreaterThan(0); } }
}
