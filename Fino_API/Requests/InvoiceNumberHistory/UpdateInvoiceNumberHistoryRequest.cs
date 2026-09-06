using FluentValidation;
namespace FintechApp.Requests.InvoiceNumberHistory
{
    public class UpdateInvoiceNumberHistoryRequest { public int InvoiceNumberHistoryID { get; set; } public string ServiceCategory { get; set; } = string.Empty; public int UserID { get; set; } public int Month { get; set; } public int Year { get; set; } public string InvoiceNo { get; set; } = string.Empty; public string InvoiceType { get; set; } = string.Empty; public int IncrementValue { get; set; } }
    public class UpdateInvoiceNumberHistoryRequestValidator : AbstractValidator<UpdateInvoiceNumberHistoryRequest> { public UpdateInvoiceNumberHistoryRequestValidator() { RuleFor(x => x.InvoiceNumberHistoryID).GreaterThan(0); } }
}
