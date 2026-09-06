using FintechApp.Requests.InvoiceNumberHistory;
using FintechApp.Responses.InvoiceNumberHistory;
namespace FintechApp.Interfaces.Services
{
    public interface IInvoiceNumberHistoryService
    {
        Task<List<InvoiceNumberHistoryResponse>> GetAll();
        Task<InvoiceNumberHistoryResponse?> GetByID(int id);
        Task<List<InvoiceNumberHistoryResponse>> GetByUserID(int userID);
        Task<string> Create(CreateInvoiceNumberHistoryRequest request);
        Task<string> Update(UpdateInvoiceNumberHistoryRequest request);
        Task<string> Delete(int id);
    }
}
