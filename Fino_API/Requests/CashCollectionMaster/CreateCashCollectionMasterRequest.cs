using FluentValidation;
namespace FintechApp.Requests.CashCollectionMaster
{
    public class CreateCashCollectionMasterRequest { public int UserID { get; set; } public decimal Outstanding { get; set; } public decimal MaxOutstanding { get; set; } }
    public class CreateCashCollectionMasterRequestValidator : AbstractValidator<CreateCashCollectionMasterRequest> { public CreateCashCollectionMasterRequestValidator() { RuleFor(x => x.UserID).GreaterThan(0); } }
}
