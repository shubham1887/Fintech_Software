using FluentValidation;
namespace FintechApp.Requests.PaymentGatewaySetting
{
    public class GetPaymentGatewaySettingRequest { public int PaymentGatewaySettingID { get; set; } }
    public class GetPaymentGatewaySettingRequestValidator : AbstractValidator<GetPaymentGatewaySettingRequest> { public GetPaymentGatewaySettingRequestValidator() { RuleFor(x => x.PaymentGatewaySettingID).GreaterThan(0); } }
}
