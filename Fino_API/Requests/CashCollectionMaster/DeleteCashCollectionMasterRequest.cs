using FluentValidation;
namespace FintechApp.Requests.CashCollectionMaster
{
    public class DeleteCashCollectionMasterRequest { public int CashCollectionMasterID { get; set; } }
    public class DeleteCashCollectionMasterRequestValidator : AbstractValidator<DeleteCashCollectionMasterRequest> { public DeleteCashCollectionMasterRequestValidator() { RuleFor(x => x.CashCollectionMasterID).GreaterThan(0); } }
}
