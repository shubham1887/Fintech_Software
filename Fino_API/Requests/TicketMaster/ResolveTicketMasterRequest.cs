using FintechApp.Messages;
using FluentValidation;

namespace FintechApp.Requests.TicketMaster
{
    /// <summary>
    /// Closing (or re-opening) a ticket. Status and the resolution note move together — a ticket
    /// marked Resolved with no note, or a note with no status change, is how support queues rot.
    /// The resolver and the timestamp are stamped from the signed-in session, not from the client.
    /// </summary>
    public class ResolveTicketMasterRequest
    {
        public int TicketMasterID { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Resolution { get; set; } = string.Empty;
    }

    public class ResolveTicketMasterRequestValidator : AbstractValidator<ResolveTicketMasterRequest>
    {
        public ResolveTicketMasterRequestValidator()
        {
            RuleFor(x => x.TicketMasterID).GreaterThan(0).WithMessage(TicketMasterMessages.InvalidValue);
            RuleFor(x => x.Status).NotEmpty().MaximumLength(20).WithMessage(TicketMasterMessages.InvalidLength);
            RuleFor(x => x.Resolution).NotEmpty().WithMessage(TicketMasterMessages.RequiredField);
        }
    }
}
