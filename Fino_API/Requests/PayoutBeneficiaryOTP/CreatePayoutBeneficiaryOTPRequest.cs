using FluentValidation;
namespace FintechApp.Requests.PayoutBeneficiaryOTP
{
    public class CreatePayoutBeneficiaryOTPRequest { public string OTP { get; set; } = string.Empty; public int PayoutBeneficaryID { get; set; } public int UserID { get; set; } public bool IsOTPVerified { get; set; } }
    public class CreatePayoutBeneficiaryOTPRequestValidator : AbstractValidator<CreatePayoutBeneficiaryOTPRequest> { public CreatePayoutBeneficiaryOTPRequestValidator() { RuleFor(x => x.UserID).GreaterThan(0); } }
}
