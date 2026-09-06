using FintechApp.Messages;
using FluentValidation;

namespace FintechApp.Requests.DMTBeneficiary
{
    public class CreateDMTBeneficiaryRequest
    {
        public string Name { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string AccountNo { get; set; } = string.Empty;
        public string IFSC { get; set; } = string.Empty;
        public string GUID { get; set; } = string.Empty;
        public string SenderMobile { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
        public int SenderID { get; set; }
        public int UserID { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class CreateDMTBeneficiaryRequestValidator : AbstractValidator<CreateDMTBeneficiaryRequest>
    {
        public CreateDMTBeneficiaryRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(DMTBeneficiaryMessages.InvalidName)
                .MaximumLength(200).WithMessage(DMTBeneficiaryMessages.InvalidLength);

            RuleFor(x => x.BankName)
                .NotEmpty().WithMessage(DMTBeneficiaryMessages.InvalidBankName)
                .MaximumLength(50).WithMessage(DMTBeneficiaryMessages.InvalidLength);

            RuleFor(x => x.AccountNo)
                .NotEmpty().WithMessage(DMTBeneficiaryMessages.InvalidAccountNo)
                .MaximumLength(30).WithMessage(DMTBeneficiaryMessages.InvalidLength);

            RuleFor(x => x.IFSC)
                .NotEmpty().WithMessage(DMTBeneficiaryMessages.InvalidIFSC)
                .Length(11).WithMessage(DMTBeneficiaryMessages.InvalidIFSC)
                .Matches("^[A-Z]{4}0[A-Z0-9]{6}$").WithMessage(DMTBeneficiaryMessages.InvalidIFSC);

            RuleFor(x => x.GUID)
                .NotEmpty().WithMessage(DMTBeneficiaryMessages.RequiredField)
                .MaximumLength(50).WithMessage(DMTBeneficiaryMessages.InvalidLength);

            RuleFor(x => x.SenderMobile)
                .NotEmpty().WithMessage(DMTBeneficiaryMessages.RequiredField)
                .Length(10).WithMessage(DMTBeneficiaryMessages.InvalidMobile)
                .Matches("^[0-9]+$").WithMessage(DMTBeneficiaryMessages.InvalidMobile);

            RuleFor(x => x.SenderID)
                .GreaterThan(0).WithMessage(DMTBeneficiaryMessages.InvalidSenderID);

            RuleFor(x => x.UserID)
                .GreaterThan(0).WithMessage(DMTBeneficiaryMessages.InvalidUserID);
        }
    }
}
