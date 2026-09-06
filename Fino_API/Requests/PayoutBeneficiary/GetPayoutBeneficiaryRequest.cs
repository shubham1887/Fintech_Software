using FluentValidation;

namespace FintechApp.Requests.PayoutBeneficiary
{
    public class GetPayoutBeneficiaryRequest { public int PayoutBeneficaryID { get; set; } public int UserID { get; set; } }
    public class GetPayoutBeneficiaryRequestValidator : AbstractValidator<GetPayoutBeneficiaryRequest>
    {
        public GetPayoutBeneficiaryRequestValidator() { RuleFor(x => x.PayoutBeneficaryID).GreaterThan(0); }
    }
}
