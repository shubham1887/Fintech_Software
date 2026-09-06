using FluentValidation;

namespace FintechApp.Requests.NPCIBank
{
    public class CreateNPCIBankRequest
    {
        public string? BankCode { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string? IFSC { get; set; }
        public string? IIN { get; set; }
        public string? ParentBank { get; set; }
        public string? BankType { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateNPCIBankRequestValidator : AbstractValidator<CreateNPCIBankRequest>
    {
        public CreateNPCIBankRequestValidator()
        {
            RuleFor(x => x.BankName).NotEmpty();
        }
    }
}
