using FluentValidation;
namespace FintechApp.Requests.PaymentModeMaster
{
    public class DeletePaymentModeMasterRequest { public int PaymentModeMasterID { get; set; } }
    public class DeletePaymentModeMasterRequestValidator : AbstractValidator<DeletePaymentModeMasterRequest> { public DeletePaymentModeMasterRequestValidator() { RuleFor(x => x.PaymentModeMasterID).GreaterThan(0); } }
}
