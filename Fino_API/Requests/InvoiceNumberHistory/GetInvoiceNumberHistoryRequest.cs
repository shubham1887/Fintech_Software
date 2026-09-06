using FluentValidation;
namespace FintechApp.Requests.InvoiceNumberHistory
{
    public class GetInvoiceNumberHistoryRequest { public int InvoiceNumberHistoryID { get; set; }
    public int UserID { get; set; } }
    public class GetInvoiceNumberHistoryRequestValidator : AbstractValidator<GetInvoiceNumberHistoryRequest> { public GetInvoiceNumberHistoryRequestValidator() { RuleFor(x => x.InvoiceNumberHistoryID).GreaterThan(0); } }
}
