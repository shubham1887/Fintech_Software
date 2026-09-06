using FluentValidation;
namespace FintechApp.Requests.Sender { public class DeleteSenderRequest { public int SenderID { get; set; } } public class DeleteSenderRequestValidator : AbstractValidator<DeleteSenderRequest> { public DeleteSenderRequestValidator() { RuleFor(x => x.SenderID).GreaterThan(0); } } }
