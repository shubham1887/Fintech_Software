using FintechApp.Messages;
using FluentValidation;
namespace FintechApp.Requests.TicketMaster
{
    public class UpdateTicketMasterRequest
    {
        public int TicketMasterID { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
    }

    public class UpdateTicketMasterRequestValidator : AbstractValidator<UpdateTicketMasterRequest>
    {
        public UpdateTicketMasterRequestValidator()
        {
            RuleFor(x => x.TicketMasterID)
                .GreaterThan(0).WithMessage(TicketMasterMessages.InvalidValue);

            RuleFor(x => x.FieldName)
                .NotEmpty().WithMessage(TicketMasterMessages.RequiredField)
                .MaximumLength(100).WithMessage(TicketMasterMessages.InvalidLength);

            RuleFor(x => x.OldValue)
                .NotEmpty().WithMessage(TicketMasterMessages.RequiredField)
                .MaximumLength(500).WithMessage(TicketMasterMessages.InvalidLength);

            RuleFor(x => x.NewValue)
                .NotEmpty().WithMessage(TicketMasterMessages.RequiredField)
                .MaximumLength(500).WithMessage(TicketMasterMessages.InvalidLength);
        }
    }
}
