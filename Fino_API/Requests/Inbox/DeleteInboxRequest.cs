using FluentValidation;
namespace FintechApp.Requests.Inbox
{
    public class DeleteInboxRequest { public int InboxID { get; set; } }
    public class DeleteInboxRequestValidator : AbstractValidator<DeleteInboxRequest> { public DeleteInboxRequestValidator() { RuleFor(x => x.InboxID).GreaterThan(0); } }
}
