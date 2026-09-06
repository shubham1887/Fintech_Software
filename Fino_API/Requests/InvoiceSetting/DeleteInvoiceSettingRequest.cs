using FluentValidation;
namespace FintechApp.Requests.InvoiceSetting
{
    public class DeleteInvoiceSettingRequest { public int InvoiceSettingID { get; set; } }
    public class DeleteInvoiceSettingRequestValidator : AbstractValidator<DeleteInvoiceSettingRequest> { public DeleteInvoiceSettingRequestValidator() { RuleFor(x => x.InvoiceSettingID).GreaterThan(0); } }
}
