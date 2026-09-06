using FluentValidation;
namespace FintechApp.Requests.PayoutVendorBeneficiaryMapping
{
    public class DeletePayoutVendorBeneficiaryMappingRequest { public int PayoutVendorBeneficiaryMappingID { get; set; } }
    public class DeletePayoutVendorBeneficiaryMappingRequestValidator : AbstractValidator<DeletePayoutVendorBeneficiaryMappingRequest> { public DeletePayoutVendorBeneficiaryMappingRequestValidator() { RuleFor(x => x.PayoutVendorBeneficiaryMappingID).GreaterThan(0); } }
}
