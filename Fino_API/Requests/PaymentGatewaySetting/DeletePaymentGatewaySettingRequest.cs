using FluentValidation;
namespace FintechApp.Requests.PaymentGatewaySetting
{
    public class DeletePaymentGatewaySettingRequest { public int PaymentGatewaySettingID { get; set; } }
    public class DeletePaymentGatewaySettingRequestValidator : AbstractValidator<DeletePaymentGatewaySettingRequest> { public DeletePaymentGatewaySettingRequestValidator() { RuleFor(x => x.PaymentGatewaySettingID).GreaterThan(0); } }
}
