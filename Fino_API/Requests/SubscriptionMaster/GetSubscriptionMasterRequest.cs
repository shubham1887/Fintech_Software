using FluentValidation;
namespace FintechApp.Requests.SubscriptionMaster { public class GetSubscriptionMasterRequest { public int SubscriptionMasterID { get; set; } } public class GetSubscriptionMasterRequestValidator : AbstractValidator<GetSubscriptionMasterRequest> { public GetSubscriptionMasterRequestValidator() { RuleFor(x => x.SubscriptionMasterID).GreaterThan(0); } } }
