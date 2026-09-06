using FluentValidation;
namespace FintechApp.Requests.ProviderOperatorMaster
{
    public class CreateProviderOperatorMasterRequest { public int ProviderID { get; set; } public int OperatorID { get; set; } public int ServiceID { get; set; } public string? Operatorcode { get; set; } public string? MarginType { get; set; } public decimal CommissionAdmin { get; set; } public int PendingCount { get; set; } public int FailureCount { get; set; } public bool IsActive { get; set; } }
    public class CreateProviderOperatorMasterRequestValidator : AbstractValidator<CreateProviderOperatorMasterRequest> { public CreateProviderOperatorMasterRequestValidator() { RuleFor(x => x.ProviderID).GreaterThan(0); RuleFor(x => x.OperatorID).GreaterThan(0); } }
}
