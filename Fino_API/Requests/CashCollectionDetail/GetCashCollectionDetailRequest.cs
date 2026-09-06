using FluentValidation;
namespace FintechApp.Requests.CashCollectionDetail
{
    public class GetCashCollectionDetailRequest { public int CashCollectionDetailID { get; set; }
    public int UserID { get; set; } }
    public class GetCashCollectionDetailRequestValidator : AbstractValidator<GetCashCollectionDetailRequest> { public GetCashCollectionDetailRequestValidator() { RuleFor(x => x.CashCollectionDetailID).GreaterThan(0); } }
}
