using FluentValidation;
namespace FintechApp.Requests.PayoutBeneficiaryOTP
{
    public class DeletePayoutBeneficiaryOTPRequest { public int PayoutBeneficiaryOTPID { get; set; } }
    public class DeletePayoutBeneficiaryOTPRequestValidator : AbstractValidator<DeletePayoutBeneficiaryOTPRequest> { public DeletePayoutBeneficiaryOTPRequestValidator() { RuleFor(x => x.PayoutBeneficiaryOTPID).GreaterThan(0); } }
}
