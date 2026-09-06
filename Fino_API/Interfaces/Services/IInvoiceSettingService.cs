using FintechApp.Requests.InvoiceSetting;
using FintechApp.Responses.InvoiceSetting;
namespace FintechApp.Interfaces.Services
{
    public interface IInvoiceSettingService
    {
        Task<List<InvoiceSettingResponse>> GetAll();
        Task<InvoiceSettingResponse?> GetByID(int id);
        Task<string> Create(CreateInvoiceSettingRequest request);
        Task<string> Update(UpdateInvoiceSettingRequest request);
        Task<string> Delete(int id);
    }
}
