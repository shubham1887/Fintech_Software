using FluentValidation;
namespace FintechApp.Requests.PayoutVendorBeneficiaryMapping
{
    public class UpdatePayoutVendorBeneficiaryMappingRequest { public int PayoutVendorBeneficiaryMappingID { get; set; } public int ProviderID { get; set; } public string PayoutVendorBeneficiaryID { get; set; } = string.Empty; public int PayoutBeneficaryID { get; set; } }
    public class UpdatePayoutVendorBeneficiaryMappingRequestValidator : AbstractValidator<UpdatePayoutVendorBeneficiaryMappingRequest> { public UpdatePayoutVendorBeneficiaryMappingRequestValidator() { RuleFor(x => x.PayoutVendorBeneficiaryMappingID).GreaterThan(0); } }
}
