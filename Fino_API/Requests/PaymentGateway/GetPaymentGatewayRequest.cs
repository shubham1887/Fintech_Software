using FluentValidation;
namespace FintechApp.Requests.PaymentGateway { public class GetPaymentGatewayRequest { public int PaymentGatewayID { get; set; } public int UserID { get; set; } } public class GetPaymentGatewayRequestValidator : AbstractValidator<GetPaymentGatewayRequest> { public GetPaymentGatewayRequestValidator() { RuleFor(x => x.PaymentGatewayID).GreaterThan(0); } } }
