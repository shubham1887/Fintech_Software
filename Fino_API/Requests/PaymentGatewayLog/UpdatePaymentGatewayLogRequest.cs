using FluentValidation;
namespace FintechApp.Requests.PaymentGatewayLog
{
    public class UpdatePaymentGatewayLogRequest { public int ID { get; set; } public int UserID { get; set; } public decimal Amount { get; set; } public string Type { get; set; } = string.Empty; public string GatewayName { get; set; } = string.Empty; public string Request { get; set; } = string.Empty; public string Response { get; set; } = string.Empty; public string OrderID { get; set; } = string.Empty; }
    public class UpdatePaymentGatewayLogRequestValidator : AbstractValidator<UpdatePaymentGatewayLogRequest> { public UpdatePaymentGatewayLogRequestValidator() { RuleFor(x => x.ID).GreaterThan(0); } }
}
