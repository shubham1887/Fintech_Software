using FluentValidation;
using FintechApp.StaticModels;
namespace FintechApp.Requests.ProviderConfigurationAdminCommission
{
    public class UpdateProviderConfigurationAdminCommissionRequest { public int ProviderConfigurationAdminCommissionID { get; set; } public int ProviderID { get; set; } public int ServiceID { get; set; } public int OperatorID { get; set; } public decimal FromAmount { get; set; } public decimal ToAmount { get; set; } public decimal AdminComm { get; set; } public string AdminCommType { get; set; } = string.Empty; public string AdminCommissionCategory { get; set; } = string.Empty; public string GSTType { get; set; } = ChargeGstType.Default; public decimal GSTRatePercent { get; set; } }
    public class UpdateProviderConfigurationAdminCommissionRequestValidator : AbstractValidator<UpdateProviderConfigurationAdminCommissionRequest> { public UpdateProviderConfigurationAdminCommissionRequestValidator() { RuleFor(x => x.ProviderConfigurationAdminCommissionID).GreaterThan(0); RuleFor(x => x.GSTType).Must(ChargeGstValidation.IsKnownType).WithMessage(ChargeGstValidation.TypeMessage); RuleFor(x => x.GSTRatePercent).InclusiveBetween(0m, 100m).WithMessage(ChargeGstValidation.RateMessage); } }
}
