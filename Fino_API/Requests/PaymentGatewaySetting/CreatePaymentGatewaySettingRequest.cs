using FluentValidation;
namespace FintechApp.Requests.PaymentGatewaySetting
{
    public class CreatePaymentGatewaySettingRequest { public string Name { get; set; } = string.Empty; public string MerchantID { get; set; } = string.Empty; public string Token { get; set; } = string.Empty; public string Slat { get; set; } = string.Empty; public string SuccessURL { get; set; } = string.Empty; public string ActionURL { get; set; } = string.Empty; public string? StatusCheckURL { get; set; } public string FailURL { get; set; } = string.Empty; public bool IsActiveWeb { get; set; } public bool IsActive { get; set; } public decimal MinimumAmount { get; set; } public decimal MaximumAmount { get; set; } public bool IsUPI { get; set; } public bool IsManualStatusChange { get; set; } public int SettlementHoldInMinutes { get; set; } }
    public class CreatePaymentGatewaySettingRequestValidator : AbstractValidator<CreatePaymentGatewaySettingRequest> { public CreatePaymentGatewaySettingRequestValidator() { RuleFor(x => x.Name).NotEmpty(); RuleFor(x => x.MerchantID).NotEmpty(); } }
}
