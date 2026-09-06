using FluentValidation;
namespace FintechApp.Requests.OperatorwiseCallbackSetting
{
    public class CreateOperatorwiseCallbackSettingRequest { public int OperatorID { get; set; } public string? Url { get; set; } public string MethodType { get; set; } = string.Empty; public string HeaderJson { get; set; } = string.Empty; public string DateFormat { get; set; } = string.Empty; public bool IsActive { get; set; } public bool IsDelete { get; set; } }
    public class CreateOperatorwiseCallbackSettingRequestValidator : AbstractValidator<CreateOperatorwiseCallbackSettingRequest> { public CreateOperatorwiseCallbackSettingRequestValidator() { RuleFor(x => x.OperatorID).GreaterThan(0); RuleFor(x => x.MethodType).NotEmpty(); } }
}
