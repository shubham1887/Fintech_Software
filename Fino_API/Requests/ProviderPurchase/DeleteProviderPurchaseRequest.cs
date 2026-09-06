using FluentValidation;
namespace FintechApp.Requests.ProviderPurchase
{
    public class DeleteProviderPurchaseRequest { public int ProviderPurchaseID { get; set; } }
    public class DeleteProviderPurchaseRequestValidator : AbstractValidator<DeleteProviderPurchaseRequest> { public DeleteProviderPurchaseRequestValidator() { RuleFor(x => x.ProviderPurchaseID).GreaterThan(0); } }
}
