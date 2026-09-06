using FluentValidation;
namespace FintechApp.Requests.InvoiceSetting
{
    public class GetInvoiceSettingRequest { public int InvoiceSettingID { get; set; } }
    public class GetInvoiceSettingRequestValidator : AbstractValidator<GetInvoiceSettingRequest> { public GetInvoiceSettingRequestValidator() { RuleFor(x => x.InvoiceSettingID).GreaterThan(0); } }
}
