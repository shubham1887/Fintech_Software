using FluentValidation;
namespace FintechApp.Requests.SubscriptionMaster { public class DeleteSubscriptionMasterRequest { public int SubscriptionMasterID { get; set; } } public class DeleteSubscriptionMasterRequestValidator : AbstractValidator<DeleteSubscriptionMasterRequest> { public DeleteSubscriptionMasterRequestValidator() { RuleFor(x => x.SubscriptionMasterID).GreaterThan(0); } } }
