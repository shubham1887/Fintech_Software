using FluentValidation;

namespace FintechApp.Requests.NPCIBank
{
    public class UpdateNPCIBankRequest
    {
        public int NPCIBankID { get; set; }
        public string? BankCode { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string? IFSC { get; set; }
        public string? IIN { get; set; }
        public string? ParentBank { get; set; }
        public string? BankType { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdateNPCIBankRequestValidator : AbstractValidator<UpdateNPCIBankRequest>
    {
        public UpdateNPCIBankRequestValidator()
        {
            RuleFor(x => x.NPCIBankID).GreaterThan(0);
            RuleFor(x => x.BankName).NotEmpty();
        }
    }
}
