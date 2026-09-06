using FintechApp.Messages;
using FluentValidation;

namespace FintechApp.Requests.TicketCategory
{
    public class GetTicketCategoryRequest
    {
        public int TicketCategoryID { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class GetTicketCategoryRequestValidator : AbstractValidator<GetTicketCategoryRequest>
    {
        public GetTicketCategoryRequestValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(100).WithMessage(TicketCategoryMessages.InvalidLength);
        }
    }
}
