using FluentValidation;
namespace FintechApp.Requests.Inbox
{
    public class GetInboxRequest { public int InboxID { get; set; }
    public int UserID { get; set; } }
    public class GetInboxRequestValidator : AbstractValidator<GetInboxRequest> { public GetInboxRequestValidator() { RuleFor(x => x.InboxID).GreaterThan(0); } }
}
