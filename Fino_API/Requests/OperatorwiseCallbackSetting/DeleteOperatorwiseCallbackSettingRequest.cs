using FluentValidation;
namespace FintechApp.Requests.OperatorwiseCallbackSetting
{
    public class DeleteOperatorwiseCallbackSettingRequest { public int OperatorwiseCallbackSettingID { get; set; } }
    public class DeleteOperatorwiseCallbackSettingRequestValidator : AbstractValidator<DeleteOperatorwiseCallbackSettingRequest> { public DeleteOperatorwiseCallbackSettingRequestValidator() { RuleFor(x => x.OperatorwiseCallbackSettingID).GreaterThan(0); } }
}
