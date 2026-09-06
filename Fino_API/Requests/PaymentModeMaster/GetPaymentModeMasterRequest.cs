using FluentValidation;
namespace FintechApp.Requests.PaymentModeMaster
{
    public class GetPaymentModeMasterRequest { public int PaymentModeMasterID { get; set; } }
    public class GetPaymentModeMasterRequestValidator : AbstractValidator<GetPaymentModeMasterRequest> { public GetPaymentModeMasterRequestValidator() { RuleFor(x => x.PaymentModeMasterID).GreaterThan(0); } }
}
