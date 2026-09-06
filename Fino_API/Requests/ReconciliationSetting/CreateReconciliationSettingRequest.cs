using FluentValidation;
namespace FintechApp.Requests.ReconciliationSetting
{
    public class CreateReconciliationSettingRequest { public int ProviderID { get; set; } public string ConsumerNumber { get; set; } = string.Empty; public string Amount { get; set; } = string.Empty; public string Status { get; set; } = string.Empty; public string WorksheetName { get; set; } = string.Empty; public string APIUserRequestID { get; set; } = string.Empty; public string SuccessStatusAs { get; set; } = string.Empty; public string FailureStatusAs { get; set; } = string.Empty; }
    public class CreateReconciliationSettingRequestValidator : AbstractValidator<CreateReconciliationSettingRequest> { public CreateReconciliationSettingRequestValidator() { RuleFor(x => x.ProviderID).GreaterThan(0); RuleFor(x => x.ConsumerNumber).NotEmpty(); } }
}
