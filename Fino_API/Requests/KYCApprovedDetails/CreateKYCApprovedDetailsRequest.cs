using FluentValidation;
namespace FintechApp.Requests.KYCApprovedDetails
{
    public class CreateKYCApprovedDetailsRequest { public int UserID { get; set; } public int ApporvedUserID { get; set; } public string? Remark { get; set; } }
    public class CreateKYCApprovedDetailsRequestValidator : AbstractValidator<CreateKYCApprovedDetailsRequest> { public CreateKYCApprovedDetailsRequestValidator() { RuleFor(x => x.UserID).GreaterThan(0); } }
}
