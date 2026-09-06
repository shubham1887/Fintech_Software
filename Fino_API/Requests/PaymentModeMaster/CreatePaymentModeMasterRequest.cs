using FluentValidation;
namespace FintechApp.Requests.PaymentModeMaster
{
    public class CreatePaymentModeMasterRequest { public string Type { get; set; } = string.Empty; public bool IsActive { get; set; } }
    public class CreatePaymentModeMasterRequestValidator : AbstractValidator<CreatePaymentModeMasterRequest> { public CreatePaymentModeMasterRequestValidator() {  } }
}
