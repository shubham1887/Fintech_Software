using FluentValidation;
namespace FintechApp.Requests.ReconciliationSetting
{
    public class DeleteReconciliationSettingRequest { public int ReconciliationSettingID { get; set; } }
    public class DeleteReconciliationSettingRequestValidator : AbstractValidator<DeleteReconciliationSettingRequest> { public DeleteReconciliationSettingRequestValidator() { RuleFor(x => x.ReconciliationSettingID).GreaterThan(0); } }
}
