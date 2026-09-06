using FluentValidation;
namespace FintechApp.Requests.BeneficiaryOTP { public class DeleteBeneficiaryOTPRequest { public int BeneficiaryOTPID { get; set; } } public class DeleteBeneficiaryOTPRequestValidator : AbstractValidator<DeleteBeneficiaryOTPRequest> { public DeleteBeneficiaryOTPRequestValidator() { RuleFor(x => x.BeneficiaryOTPID).GreaterThan(0); } } }
