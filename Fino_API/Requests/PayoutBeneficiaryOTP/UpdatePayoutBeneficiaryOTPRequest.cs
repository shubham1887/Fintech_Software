using FluentValidation;
namespace FintechApp.Requests.PayoutBeneficiaryOTP
{
    public class UpdatePayoutBeneficiaryOTPRequest { public int PayoutBeneficiaryOTPID { get; set; } public string OTP { get; set; } = string.Empty; public int PayoutBeneficaryID { get; set; } public int UserID { get; set; } public bool IsOTPVerified { get; set; } }
    public class UpdatePayoutBeneficiaryOTPRequestValidator : AbstractValidator<UpdatePayoutBeneficiaryOTPRequest> { public UpdatePayoutBeneficiaryOTPRequestValidator() { RuleFor(x => x.PayoutBeneficiaryOTPID).GreaterThan(0); } }
}
