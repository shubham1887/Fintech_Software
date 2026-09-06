using FluentValidation;
namespace FintechApp.Requests.ReconciliationSetting
{
    public class UpdateReconciliationSettingRequest { public int ReconciliationSettingID { get; set; } public int ProviderID { get; set; } public string ConsumerNumber { get; set; } = string.Empty; public string Amount { get; set; } = string.Empty; public string Status { get; set; } = string.Empty; public string WorksheetName { get; set; } = string.Empty; public string APIUserRequestID { get; set; } = string.Empty; public string SuccessStatusAs { get; set; } = string.Empty; public string FailureStatusAs { get; set; } = string.Empty; }
    public class UpdateReconciliationSettingRequestValidator : AbstractValidator<UpdateReconciliationSettingRequest> { public UpdateReconciliationSettingRequestValidator() { RuleFor(x => x.ReconciliationSettingID).GreaterThan(0); } }
}
