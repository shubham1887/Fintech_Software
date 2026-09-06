using FluentValidation;
namespace FintechApp.Requests.CashCollectionMaster
{
    public class GetCashCollectionMasterRequest { public int CashCollectionMasterID { get; set; }
    public int UserID { get; set; } }
    public class GetCashCollectionMasterRequestValidator : AbstractValidator<GetCashCollectionMasterRequest> { public GetCashCollectionMasterRequestValidator() { RuleFor(x => x.CashCollectionMasterID).GreaterThan(0); } }
}
