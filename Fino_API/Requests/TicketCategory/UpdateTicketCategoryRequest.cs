using FintechApp.Messages;
using FluentValidation;

namespace FintechApp.Requests.TicketCategory
{
    public class UpdateTicketCategoryRequest
    {
        public int TicketCategoryID { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
    }

    public class UpdateTicketCategoryRequestValidator : AbstractValidator<UpdateTicketCategoryRequest>
    {
        public UpdateTicketCategoryRequestValidator()
        {
            RuleFor(x => x.TicketCategoryID)
                .GreaterThan(0).WithMessage(TicketCategoryMessages.InvalidValue);

            RuleFor(x => x.FieldName)
                .NotEmpty().WithMessage(TicketCategoryMessages.RequiredField)
                .MaximumLength(100).WithMessage(TicketCategoryMessages.InvalidLength);

            RuleFor(x => x.OldValue)
                .NotEmpty().WithMessage(TicketCategoryMessages.RequiredField)
                .MaximumLength(100).WithMessage(TicketCategoryMessages.InvalidLength);

            RuleFor(x => x.NewValue)
                .NotEmpty().WithMessage(TicketCategoryMessages.RequiredField)
                .MaximumLength(100).WithMessage(TicketCategoryMessages.InvalidLength);
        }
    }
}
