using FluentValidation;
namespace FintechApp.Requests.Sender { public class GetSenderRequest { public int SenderID { get; set; } public int UserID { get; set; } } public class GetSenderRequestValidator : AbstractValidator<GetSenderRequest> { public GetSenderRequestValidator() { RuleFor(x => x.SenderID).GreaterThan(0); } } }
