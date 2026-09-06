using FluentValidation;

namespace FintechApp.Requests.PayoutBeneficiary
{
    public class DeletePayoutBeneficiaryRequest { public int PayoutBeneficaryID { get; set; } }
    public class DeletePayoutBeneficiaryRequestValidator : AbstractValidator<DeletePayoutBeneficiaryRequest>
    {
        public DeletePayoutBeneficiaryRequestValidator() { RuleFor(x => x.PayoutBeneficaryID).GreaterThan(0); }
    }
}
