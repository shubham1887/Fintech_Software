using FluentValidation;
namespace FintechApp.Requests.PaymentModeMaster
{
    public class UpdatePaymentModeMasterRequest { public int PaymentModeMasterID { get; set; } public string Type { get; set; } = string.Empty; public bool IsActive { get; set; } }
    public class UpdatePaymentModeMasterRequestValidator : AbstractValidator<UpdatePaymentModeMasterRequest> { public UpdatePaymentModeMasterRequestValidator() { RuleFor(x => x.PaymentModeMasterID).GreaterThan(0); } }
}
