using FluentValidation;
namespace FintechApp.Requests.ProviderConfigurationAdminCommission
{
    public class CreateProviderConfigurationAdminCommissionRequest { public int ProviderID { get; set; } public int ServiceID { get; set; } public int OperatorID { get; set; } public decimal FromAmount { get; set; } public decimal ToAmount { get; set; } public decimal AdminComm { get; set; } public string AdminCommType { get; set; } = string.Empty; public string AdminCommissionCategory { get; set; } = string.Empty; }
    public class CreateProviderConfigurationAdminCommissionRequestValidator : AbstractValidator<CreateProviderConfigurationAdminCommissionRequest> { public CreateProviderConfigurationAdminCommissionRequestValidator() { RuleFor(x => x.ProviderID).GreaterThan(0); } }
}
