using FluentValidation;
namespace FintechApp.Requests.UpiSender { public class GetUpiSenderRequest { public int UpiSenderID { get; set; } public int UserID { get; set; } } public class GetUpiSenderRequestValidator : AbstractValidator<GetUpiSenderRequest> { public GetUpiSenderRequestValidator() { RuleFor(x => x.UpiSenderID).GreaterThan(0); } } }
