using FluentValidation;

namespace FintechApp.Requests.NPCIBank
{
    public class DeleteNPCIBankRequest
    {
        public int NPCIBankID { get; set; }
    }

    public class DeleteNPCIBankRequestValidator : AbstractValidator<DeleteNPCIBankRequest>
    {
        public DeleteNPCIBankRequestValidator()
        {
            RuleFor(x => x.NPCIBankID).GreaterThan(0);
        }
    }
}
