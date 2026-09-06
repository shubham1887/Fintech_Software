using FluentValidation;
namespace FintechApp.Requests.OperatorwiseCallbackSetting
{
    public class GetOperatorwiseCallbackSettingRequest { public int OperatorwiseCallbackSettingID { get; set; } public int OperatorID { get; set; } }
    public class GetOperatorwiseCallbackSettingRequestValidator : AbstractValidator<GetOperatorwiseCallbackSettingRequest> { public GetOperatorwiseCallbackSettingRequestValidator() { RuleFor(x => x.OperatorwiseCallbackSettingID).GreaterThan(0); } }
}
