using FluentValidation;
namespace FintechApp.Requests.OperatorwiseCallbackSetting
{
    public class UpdateOperatorwiseCallbackSettingRequest { public int OperatorwiseCallbackSettingID { get; set; } public int OperatorID { get; set; } public string? Url { get; set; } public string MethodType { get; set; } = string.Empty; public string HeaderJson { get; set; } = string.Empty; public string DateFormat { get; set; } = string.Empty; public bool IsActive { get; set; } public bool IsDelete { get; set; } }
    public class UpdateOperatorwiseCallbackSettingRequestValidator : AbstractValidator<UpdateOperatorwiseCallbackSettingRequest> { public UpdateOperatorwiseCallbackSettingRequestValidator() { RuleFor(x => x.OperatorwiseCallbackSettingID).GreaterThan(0); } }
}
