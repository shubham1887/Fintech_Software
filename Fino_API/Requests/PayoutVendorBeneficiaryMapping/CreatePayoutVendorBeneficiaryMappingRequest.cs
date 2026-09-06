using FluentValidation;
namespace FintechApp.Requests.PayoutVendorBeneficiaryMapping
{
    public class CreatePayoutVendorBeneficiaryMappingRequest { public int ProviderID { get; set; } public string PayoutVendorBeneficiaryID { get; set; } = string.Empty; public int PayoutBeneficaryID { get; set; } }
    public class CreatePayoutVendorBeneficiaryMappingRequestValidator : AbstractValidator<CreatePayoutVendorBeneficiaryMappingRequest> { public CreatePayoutVendorBeneficiaryMappingRequestValidator() {  } }
}
