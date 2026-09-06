using FluentValidation;

namespace FintechApp.Requests.NPCIBank
{
    public class GetNPCIBankRequest
    {
        public int NPCIBankID { get; set; }
    }

    public class GetNPCIBankRequestValidator : AbstractValidator<GetNPCIBankRequest>
    {
        public GetNPCIBankRequestValidator()
        {
            RuleFor(x => x.NPCIBankID).GreaterThan(0);
        }
    }
}
