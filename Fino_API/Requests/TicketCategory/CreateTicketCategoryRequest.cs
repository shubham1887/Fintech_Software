using FintechApp.Messages;
using FluentValidation;

namespace FintechApp.Requests.TicketCategory
{
    public class CreateTicketCategoryRequest
    {
        public string Name { get; set; } = string.Empty;
    }

    public class CreateTicketCategoryRequestValidator : AbstractValidator<CreateTicketCategoryRequest>
    {
        public CreateTicketCategoryRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(TicketCategoryMessages.RequiredField)
                .MaximumLength(100).WithMessage(TicketCategoryMessages.InvalidLength);
        }
    }
}
