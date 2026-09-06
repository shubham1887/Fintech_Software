using FluentValidation;
namespace FintechApp.Requests.PaymentGateway { public class DeletePaymentGatewayRequest { public int PaymentGatewayID { get; set; } } public class DeletePaymentGatewayRequestValidator : AbstractValidator<DeletePaymentGatewayRequest> { public DeletePaymentGatewayRequestValidator() { RuleFor(x => x.PaymentGatewayID).GreaterThan(0); } } }
