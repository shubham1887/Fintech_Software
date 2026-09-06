using FluentValidation;
namespace FintechApp.Requests.PayoutVendorBeneficiaryMapping
{
    public class GetPayoutVendorBeneficiaryMappingRequest { public int PayoutVendorBeneficiaryMappingID { get; set; } }
    public class GetPayoutVendorBeneficiaryMappingRequestValidator : AbstractValidator<GetPayoutVendorBeneficiaryMappingRequest> { public GetPayoutVendorBeneficiaryMappingRequestValidator() { RuleFor(x => x.PayoutVendorBeneficiaryMappingID).GreaterThan(0); } }
}
