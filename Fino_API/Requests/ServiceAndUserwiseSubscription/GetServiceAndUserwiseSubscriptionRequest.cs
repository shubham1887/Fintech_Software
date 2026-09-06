using FluentValidation;
namespace FintechApp.Requests.ServiceAndUserwiseSubscription
{
    public class GetServiceAndUserwiseSubscriptionRequest { public int ServiceAndUserwiseSubscriptionID { get; set; }
    public int UserID { get; set; } }
    public class GetServiceAndUserwiseSubscriptionRequestValidator : AbstractValidator<GetServiceAndUserwiseSubscriptionRequest> { public GetServiceAndUserwiseSubscriptionRequestValidator() { RuleFor(x => x.ServiceAndUserwiseSubscriptionID).GreaterThan(0); } }
}
