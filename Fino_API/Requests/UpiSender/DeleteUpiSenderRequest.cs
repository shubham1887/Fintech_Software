using FluentValidation;
namespace FintechApp.Requests.UpiSender { public class DeleteUpiSenderRequest { public int UpiSenderID { get; set; } } public class DeleteUpiSenderRequestValidator : AbstractValidator<DeleteUpiSenderRequest> { public DeleteUpiSenderRequestValidator() { RuleFor(x => x.UpiSenderID).GreaterThan(0); } } }
