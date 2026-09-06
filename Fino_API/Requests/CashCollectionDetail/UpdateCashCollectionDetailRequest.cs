using FluentValidation;
namespace FintechApp.Requests.CashCollectionDetail
{
    public class UpdateCashCollectionDetailRequest { public int CashCollectionDetailID { get; set; } public int UserID { get; set; } public int ParentID { get; set; } public decimal OutstandingOpening { get; set; } public decimal OutstandingAmount { get; set; } public decimal OutstandingClosing { get; set; } public string ParentRemark { get; set; } = string.Empty; public string PayMode { get; set; } = string.Empty; public string Credit { get; set; } = string.Empty; public string Debit { get; set; } = string.Empty; public string UTRNumber { get; set; } = string.Empty; public string Type { get; set; } = string.Empty; public bool IsDelete { get; set; } }
    public class UpdateCashCollectionDetailRequestValidator : AbstractValidator<UpdateCashCollectionDetailRequest> { public UpdateCashCollectionDetailRequestValidator() { RuleFor(x => x.CashCollectionDetailID).GreaterThan(0); } }
}
