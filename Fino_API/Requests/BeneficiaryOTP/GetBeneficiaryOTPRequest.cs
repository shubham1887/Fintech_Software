using FluentValidation;
namespace FintechApp.Requests.BeneficiaryOTP { public class GetBeneficiaryOTPRequest { public int BeneficiaryOTPID { get; set; } public int UserID { get; set; } } public class GetBeneficiaryOTPRequestValidator : AbstractValidator<GetBeneficiaryOTPRequest> { public GetBeneficiaryOTPRequestValidator() { RuleFor(x => x.BeneficiaryOTPID).GreaterThan(0); } } }
