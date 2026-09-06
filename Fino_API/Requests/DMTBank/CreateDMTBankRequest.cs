using FluentValidation;

namespace FintechApp.Requests.DMTBank
{
    public class CreateDMTBankRequest
    {
        public string Name { get; set; } = string.Empty;
        public string DefaultIFSC { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public bool IsIMPS { get; set; } = true;
    }

    public class CreateDMTBankRequestValidator : AbstractValidator<CreateDMTBankRequest>
    {
        public CreateDMTBankRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.DefaultIFSC).MaximumLength(50);
        }
    }
}
