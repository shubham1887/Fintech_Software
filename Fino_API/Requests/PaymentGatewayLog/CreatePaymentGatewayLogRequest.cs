using FluentValidation;
namespace FintechApp.Requests.PaymentGatewayLog
{
    public class CreatePaymentGatewayLogRequest { public int UserID { get; set; } public decimal Amount { get; set; } public string Type { get; set; } = string.Empty; public string GatewayName { get; set; } = string.Empty; public string Request { get; set; } = string.Empty; public string Response { get; set; } = string.Empty; public string OrderID { get; set; } = string.Empty; }
    public class CreatePaymentGatewayLogRequestValidator : AbstractValidator<CreatePaymentGatewayLogRequest> { public CreatePaymentGatewayLogRequestValidator() { RuleFor(x => x.UserID).GreaterThan(0); } }
}
