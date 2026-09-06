using FluentValidation;
namespace FintechApp.Requests.CashCollectionDetail
{
    public class DeleteCashCollectionDetailRequest { public int CashCollectionDetailID { get; set; } }
    public class DeleteCashCollectionDetailRequestValidator : AbstractValidator<DeleteCashCollectionDetailRequest> { public DeleteCashCollectionDetailRequestValidator() { RuleFor(x => x.CashCollectionDetailID).GreaterThan(0); } }
}
