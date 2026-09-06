using FluentValidation;
using FintechApp.Messages;

namespace FintechApp.Requests.TicketMaster
{
    public class GetTicketMasterRequest
    {
        public int TicketMasterID { get; set; }
        public int TicketCategoryID { get; set; }
        public int UserID { get; set; }
        public int UserTypeID { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class GetTicketMasterRequestValidator : AbstractValidator<GetTicketMasterRequest>
    {
        public GetTicketMasterRequestValidator()
        {
            RuleFor(x => x.Status)
                .MaximumLength(20).WithMessage(TicketMasterMessages.InvalidLength);
        }
    }
}