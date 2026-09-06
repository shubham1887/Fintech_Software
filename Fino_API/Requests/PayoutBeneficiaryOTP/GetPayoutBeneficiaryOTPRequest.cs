using FluentValidation;
namespace FintechApp.Requests.PayoutBeneficiaryOTP
{
    public class GetPayoutBeneficiaryOTPRequest { public int PayoutBeneficiaryOTPID { get; set; }
    public int UserID { get; set; } }
    public class GetPayoutBeneficiaryOTPRequestValidator : AbstractValidator<GetPayoutBeneficiaryOTPRequest> { public GetPayoutBeneficiaryOTPRequestValidator() { RuleFor(x => x.PayoutBeneficiaryOTPID).GreaterThan(0); } }
}
