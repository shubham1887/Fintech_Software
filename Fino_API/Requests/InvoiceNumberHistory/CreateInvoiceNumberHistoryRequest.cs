using FluentValidation;
namespace FintechApp.Requests.InvoiceNumberHistory
{
    public class CreateInvoiceNumberHistoryRequest { public string ServiceCategory { get; set; } = string.Empty; public int UserID { get; set; } public int Month { get; set; } public int Year { get; set; } public string InvoiceNo { get; set; } = string.Empty; public string InvoiceType { get; set; } = string.Empty; public int IncrementValue { get; set; } }
    public class CreateInvoiceNumberHistoryRequestValidator : AbstractValidator<CreateInvoiceNumberHistoryRequest> { public CreateInvoiceNumberHistoryRequestValidator() { RuleFor(x => x.UserID).GreaterThan(0); } }
}
