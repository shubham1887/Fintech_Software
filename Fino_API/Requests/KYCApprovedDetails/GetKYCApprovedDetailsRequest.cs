using FluentValidation;
namespace FintechApp.Requests.KYCApprovedDetails
{
    public class GetKYCApprovedDetailsRequest { public int KYCApprovedDetailsID { get; set; } public int UserID { get; set; } }
    public class GetKYCApprovedDetailsRequestValidator : AbstractValidator<GetKYCApprovedDetailsRequest> { public GetKYCApprovedDetailsRequestValidator() { RuleFor(x => x.KYCApprovedDetailsID).GreaterThan(0); } }
}
