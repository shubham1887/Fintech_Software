using FluentValidation;
namespace FintechApp.Requests.PaymentGatewayLog
{
    public class GetPaymentGatewayLogRequest { public int ID { get; set; }
    public int UserID { get; set; } }
    public class GetPaymentGatewayLogRequestValidator : AbstractValidator<GetPaymentGatewayLogRequest> { public GetPaymentGatewayLogRequestValidator() { RuleFor(x => x.ID).GreaterThan(0); } }
}
