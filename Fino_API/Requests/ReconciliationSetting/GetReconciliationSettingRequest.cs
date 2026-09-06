using FluentValidation;
namespace FintechApp.Requests.ReconciliationSetting
{
    public class GetReconciliationSettingRequest { public int ReconciliationSettingID { get; set; } public int ProviderID { get; set; } }
    public class GetReconciliationSettingRequestValidator : AbstractValidator<GetReconciliationSettingRequest> { public GetReconciliationSettingRequestValidator() { RuleFor(x => x.ReconciliationSettingID).GreaterThan(0); } }
}
