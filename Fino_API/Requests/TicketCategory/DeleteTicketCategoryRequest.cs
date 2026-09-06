using FintechApp.Messages;
using FluentValidation;

namespace FintechApp.Requests.TicketCategory
{
    public class DeleteTicketCategoryRequest
    {
        public int TicketCategoryID { get; set; }
    }

    public class DeleteTicketCategoryRequestValidator : AbstractValidator<DeleteTicketCategoryRequest>
    {
        public DeleteTicketCategoryRequestValidator()
        {
            RuleFor(x => x.TicketCategoryID)
                .GreaterThan(0).WithMessage(TicketCategoryMessages.InvalidValue);
        }
    }

}
