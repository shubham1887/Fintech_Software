using FluentValidation;
namespace FintechApp.Requests.ProviderPurchase
{
    public class GetProviderPurchaseRequest { public int ProviderPurchaseID { get; set; }
    public int ProviderID { get; set; } }
    public class GetProviderPurchaseRequestValidator : AbstractValidator<GetProviderPurchaseRequest> { public GetProviderPurchaseRequestValidator() { RuleFor(x => x.ProviderPurchaseID).GreaterThan(0); } }
}
