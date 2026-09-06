using FluentValidation;
namespace FintechApp.Requests.ProviderPurchase
{
    public class CreateProviderPurchaseRequest { public int ProviderID { get; set; } public int WalletID { get; set; } public decimal Amount { get; set; } public string Remark { get; set; } = string.Empty; public DateTime PurchaseDate { get; set; } = DateTime.MinValue; public int PurchaseDOCInt { get; set; } public bool IsDelete { get; set; } }
    public class CreateProviderPurchaseRequestValidator : AbstractValidator<CreateProviderPurchaseRequest> { public CreateProviderPurchaseRequestValidator() { RuleFor(x => x.ProviderID).GreaterThan(0); } }
}
