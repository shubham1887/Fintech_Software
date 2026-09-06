using FluentValidation;
namespace FintechApp.Requests.KYCApprovedDetails
{
    public class UpdateKYCApprovedDetailsRequest { public int KYCApprovedDetailsID { get; set; } public int UserID { get; set; } public int ApporvedUserID { get; set; } public string? Remark { get; set; } }
    public class UpdateKYCApprovedDetailsRequestValidator : AbstractValidator<UpdateKYCApprovedDetailsRequest> { public UpdateKYCApprovedDetailsRequestValidator() { RuleFor(x => x.KYCApprovedDetailsID).GreaterThan(0); } }
}
