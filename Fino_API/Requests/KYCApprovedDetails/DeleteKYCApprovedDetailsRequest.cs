using FluentValidation;
namespace FintechApp.Requests.KYCApprovedDetails
{
    public class DeleteKYCApprovedDetailsRequest { public int KYCApprovedDetailsID { get; set; } }
    public class DeleteKYCApprovedDetailsRequestValidator : AbstractValidator<DeleteKYCApprovedDetailsRequest> { public DeleteKYCApprovedDetailsRequestValidator() { RuleFor(x => x.KYCApprovedDetailsID).GreaterThan(0); } }
}
