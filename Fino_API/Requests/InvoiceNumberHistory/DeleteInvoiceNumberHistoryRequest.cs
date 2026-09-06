using FluentValidation;
namespace FintechApp.Requests.InvoiceNumberHistory
{
    public class DeleteInvoiceNumberHistoryRequest { public int InvoiceNumberHistoryID { get; set; } }
    public class DeleteInvoiceNumberHistoryRequestValidator : AbstractValidator<DeleteInvoiceNumberHistoryRequest> { public DeleteInvoiceNumberHistoryRequestValidator() { RuleFor(x => x.InvoiceNumberHistoryID).GreaterThan(0); } }
}
