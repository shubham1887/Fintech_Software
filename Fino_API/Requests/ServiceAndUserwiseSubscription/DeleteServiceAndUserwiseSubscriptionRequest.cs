using FluentValidation;
namespace FintechApp.Requests.ServiceAndUserwiseSubscription
{
    public class DeleteServiceAndUserwiseSubscriptionRequest { public int ServiceAndUserwiseSubscriptionID { get; set; } }
    public class DeleteServiceAndUserwiseSubscriptionRequestValidator : AbstractValidator<DeleteServiceAndUserwiseSubscriptionRequest> { public DeleteServiceAndUserwiseSubscriptionRequestValidator() { RuleFor(x => x.ServiceAndUserwiseSubscriptionID).GreaterThan(0); } }
}
