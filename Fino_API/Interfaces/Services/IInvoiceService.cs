using FintechApp.Requests.Invoice;
using FintechApp.Responses.Invoice;

namespace FintechApp.Interfaces.Services
{
    public interface IInvoiceService
    {
        Task<(string failureMessage, List<InvoiceReportViewModel>? data)> Select(SelectInvoiceRequest request);
        Task<(string failureMessage, string html)> SelectByType(SelectByTypeRequest request);
    }
}
