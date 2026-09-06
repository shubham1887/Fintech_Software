using FluentValidation;
namespace FintechApp.Requests.CashCollectionMaster
{
    public class UpdateCashCollectionMasterRequest { public int CashCollectionMasterID { get; set; } public int UserID { get; set; } public decimal Outstanding { get; set; } public decimal MaxOutstanding { get; set; } }
    public class UpdateCashCollectionMasterRequestValidator : AbstractValidator<UpdateCashCollectionMasterRequest> { public UpdateCashCollectionMasterRequestValidator() { RuleFor(x => x.CashCollectionMasterID).GreaterThan(0); } }
}
