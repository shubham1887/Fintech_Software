using FintechApp.Messages;
using FluentValidation;

namespace FintechApp.Requests.DMTBeneficiary
{
    public class DeleteDMTBeneficiaryRequest
    {
        public int BeneficiaryID { get; set; }
    }

    public class DeleteDMTBeneficiaryRequestValidator : AbstractValidator<DeleteDMTBeneficiaryRequest>
    {
        public DeleteDMTBeneficiaryRequestValidator()
        {
            RuleFor(x => x.BeneficiaryID)
                .GreaterThan(0).WithMessage(DMTBeneficiaryMessages.InvalidValue);
        }
    }
}
