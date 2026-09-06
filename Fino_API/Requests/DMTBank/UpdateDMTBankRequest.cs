using FluentValidation;

namespace FintechApp.Requests.DMTBank
{
    public class UpdateDMTBankRequest
    {
        public int DMTBankID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DefaultIFSC { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsIMPS { get; set; }
    }

    public class UpdateDMTBankRequestValidator : AbstractValidator<UpdateDMTBankRequest>
    {
        public UpdateDMTBankRequestValidator()
        {
            RuleFor(x => x.DMTBankID).GreaterThan(0);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.DefaultIFSC).MaximumLength(50);
        }
    }
}
