using FintechApp.Messages;
using FluentValidation;

namespace FintechApp.Requests.TicketMaster
{
    public class DeleteTicketMasterRequest
    {
        public int TicketMasterID { get; set; }
    }

    public class DeleteTicketMasterRequestValidator : AbstractValidator<DeleteTicketMasterRequest>
    {
        public DeleteTicketMasterRequestValidator()
        {
            RuleFor(x => x.TicketMasterID)
                .GreaterThan(0).WithMessage(TicketMasterMessages.InvalidValue);
        }
    }
}
