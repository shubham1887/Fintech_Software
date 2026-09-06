using FluentValidation;
namespace FintechApp.Requests.PaymentGatewayLog
{
    public class DeletePaymentGatewayLogRequest { public int ID { get; set; } }
    public class DeletePaymentGatewayLogRequestValidator : AbstractValidator<DeletePaymentGatewayLogRequest> { public DeletePaymentGatewayLogRequestValidator() { RuleFor(x => x.ID).GreaterThan(0); } }
}
