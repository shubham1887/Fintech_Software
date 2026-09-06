using FintechApp.Messages;
using FluentValidation;

namespace FintechApp.Requests.DMTBeneficiary
{
    public class UpdateDMTBeneficiaryRequest
    {
        public int BeneficiaryID { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
    }

    public class UpdateDMTBeneficiaryRequestValidator : AbstractValidator<UpdateDMTBeneficiaryRequest>
    {
        public UpdateDMTBeneficiaryRequestValidator()
        {
            RuleFor(x => x.BeneficiaryID)
                .GreaterThan(0).WithMessage(DMTBeneficiaryMessages.InvalidValue);

            RuleFor(x => x.FieldName)
                .NotEmpty().WithMessage(DMTBeneficiaryMessages.RequiredField);

            RuleFor(x => x.NewValue)
                .NotEmpty().WithMessage(DMTBeneficiaryMessages.RequiredField);
        }
    }
}
