using FluentValidation;

namespace FintechApp.Requests.DMTBank
{
    public class DeleteDMTBankRequest
    {
        public int DMTBankID { get; set; }
    }

    public class DeleteDMTBankRequestValidator : AbstractValidator<DeleteDMTBankRequest>
    {
        public DeleteDMTBankRequestValidator()
        {
            RuleFor(x => x.DMTBankID).GreaterThan(0);
        }
    }
}
