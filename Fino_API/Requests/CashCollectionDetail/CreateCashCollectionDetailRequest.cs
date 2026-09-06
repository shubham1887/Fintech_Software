using FluentValidation;
namespace FintechApp.Requests.CashCollectionDetail
{
    public class CreateCashCollectionDetailRequest { public int UserID { get; set; } public int ParentID { get; set; } public decimal OutstandingOpening { get; set; } public decimal OutstandingAmount { get; set; } public decimal OutstandingClosing { get; set; } public string ParentRemark { get; set; } = string.Empty; public string PayMode { get; set; } = string.Empty; public string Credit { get; set; } = string.Empty; public string Debit { get; set; } = string.Empty; public string UTRNumber { get; set; } = string.Empty; public string Type { get; set; } = string.Empty; public bool IsDelete { get; set; } }
    public class CreateCashCollectionDetailRequestValidator : AbstractValidator<CreateCashCollectionDetailRequest> { public CreateCashCollectionDetailRequestValidator() { RuleFor(x => x.UserID).GreaterThan(0); } }
}
